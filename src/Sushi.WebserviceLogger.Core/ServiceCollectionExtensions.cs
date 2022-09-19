using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWebserviceLogging<T>(this IServiceCollection services, Action<LoggerOptions<T>> configureOptions) where T : LogItem, new()
        {
            services.TryAddTransient<Logger<T>>();
            services.Configure(configureOptions);
            return services;
        }
    }
}
