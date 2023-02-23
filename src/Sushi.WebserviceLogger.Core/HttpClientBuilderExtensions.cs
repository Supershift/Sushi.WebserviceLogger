using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Core
{
    /// <summary>
    /// Extension methods to configure a <see cref="IHttpClientBuilder"/> for outgoing webservice logging.
    /// </summary>
    public static class HttpClientBuilderExtensions
    {
        /// <summary>
        /// Adds registrations for outgoing webservice logging to <see cref="IHttpClientBuilder.Services"/> and adds a <see cref="WebserviceLoggingHandler{LogItem}"/> to the <see cref="HttpClient"/>.
        /// </summary>        
        /// <param name="clientBuilder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddWebserviceLogging(this IHttpClientBuilder clientBuilder, Action<LoggerOptions<LogItem>> configureOptions)
        {
            return AddWebserviceLogging<LogItem>(clientBuilder, configureOptions);
        }


        /// <summary>
        /// Adds registrations for outgoing webservice logging to <see cref="IHttpClientBuilder.Services"/> and adds a <see cref="WebserviceLoggingHandler{T}"/> to the <see cref="HttpClient"/>.
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
