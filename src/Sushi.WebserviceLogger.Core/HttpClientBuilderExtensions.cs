using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Core
{
    public static class HttpClientBuilderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TLogItem"></typeparam>
        /// <param name="clientBuilder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddWebserviceLogging<TLogItem>(this IHttpClientBuilder clientBuilder, Action<LoggerOptions<TLogItem>> configureOptions) where TLogItem : LogItem, new()
        {
            var services = clientBuilder.Services;
            var name = clientBuilder.Name;

            // add http context accessor
            services.AddHttpContextAccessor();

            // register logger
            services.TryAddTransient(typeof(Logger<>));

            // register options
            services.Configure(name, configureOptions);

            clientBuilder.AddHttpMessageHandler(s =>
            {
                // get options by name
                var optionsMonitor = s.GetRequiredService<IOptionsMonitor<LoggerOptions<TLogItem>>>();
                var options = optionsMonitor.Get(name);

                // create a logger, inject options directly
                var logger = ActivatorUtilities.CreateInstance<Logger<TLogItem>>(s, options);

                // create a message handler, inject logger directly
                var messageHandler = ActivatorUtilities.CreateInstance<WebserviceLoggingHandler<TLogItem>>(s, logger);

                return messageHandler;
            });

            return clientBuilder;
        }
    }
}
