using Microsoft.Extensions.DependencyInjection;
using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Filter
{
    /// <summary>
    /// Extension methods for configuring <see cref="MessageLoggerFilter{T}"/> services.
    /// </summary>
    public static class MessageLoggerFilterServiceCollectionExtensions
    {   
        /// <summary>
        /// Adds a default implementation for the <see cref="MessageLoggerFilter{T}"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMessageLoggerFilter<T>(this IServiceCollection services, Action<MessageLoggerFilterOptions<T>> options) where T : LogItem, new()
        {   
            services.AddHttpContextAccessor();

            services.AddOptions<MessageLoggerFilterOptions<T>>()
                .Configure(options);
            // add the filter
            services.AddScoped(typeof(MessageLoggerFilter<>));

            return services;   
        }
    }
}
