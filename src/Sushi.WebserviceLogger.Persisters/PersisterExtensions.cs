using Microsoft.Extensions.DependencyInjection;
using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Persisters
{
    public static class PersisterExtensions
    {
        /// <summary>
        /// Adds registration to add a <see cref="QueuePersister"/>. The persister is consumed by a hosted service implementation, <see cref="QueueProcessorHostedService"/>.
        /// Requires a service registration for <see cref="Nest.IElasticClient"/>.
        /// </summary>        
        /// <returns></returns>
        public static IServiceCollection AddQueuePersister(this IServiceCollection services, Action<QueueProcessorOptions> options) 
        {
            // register persister
            services.AddSingleton<QueuePersister>();
            services.AddSingleton<ILogItemPersister, QueuePersister>(s => s.GetRequiredService<QueuePersister>());
            services.AddHttpClient
            // register consumer of persister (a hosted services) and its options
            services.AddOptions<QueueProcessorOptions>()
            .Configure(options)
            .ValidateDataAnnotations();

            services.AddHostedService<QueueProcessorHostedService>();

            return services;
        }
    }
}
