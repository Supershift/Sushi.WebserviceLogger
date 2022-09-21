using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Filter
{
    /// <summary>
    /// Extension methods for configuring <see cref="WebserviceLoggerFilter{T}"/> services.
    /// </summary>
    public static class WebserviceLoggerFilterServiceCollectionExtensions
    {
        /// <summary>
        /// Adds incoming webservice logging using <see cref="WebserviceLoggerFilter{T}"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddWebserviceLoggerFilter<T>(this IServiceCollection services) where T : LogItem, new()
        {
            return services.AddWebserviceLoggerFilter<T>(null);
        }

        /// <summary>
        /// Adds incoming webservice logging using <see cref="WebserviceLoggerFilter{T}"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddWebserviceLoggerFilter<T>(this IServiceCollection services, Action<FilterConfigurationBuilder<T>> configurationBuilder) where T : LogItem, new()
        {
            // check if a persister is registrated
            if (services.Any(x => x.ServiceType == typeof(ILogItemPersister)) == false)
            {
                throw new InvalidOperationException($"Cannot register webservice logger filter without a registration for {nameof(ILogItemPersister)}. Register a persister first.");
            }
            
            // create new builder
            var builder = new FilterConfigurationBuilder<T>();

            // call action to configure builder
            if(configurationBuilder != null)
                configurationBuilder(builder);
            
            // add http context accesssor
            services.AddHttpContextAccessor();

            // determine name of FilterLogger<T>, to register options
            string name = typeof(FilterLogger<T>).FullName;
            
            // register options for logger
            var loggerOptionsBuilder = services.AddOptions<LoggerOptions<T>>(name);
            if(builder.LoggerOptions != null)
                loggerOptionsBuilder.Configure(builder.LoggerOptions);

            // add logger
            services.TryAddTransient<FilterLogger<T>>();

            // register options for filter
            var filterOptionsBuilder = services.AddOptions<WebserviceLoggerFilterOptions<T>>();
            if(builder.FilterOptions != null)
                filterOptionsBuilder.Configure(builder.FilterOptions);

            // add the filter, one instance per incoming request
            services.AddScoped<WebserviceLoggerFilter<T>>();

            return services;   
        }
    }
}
