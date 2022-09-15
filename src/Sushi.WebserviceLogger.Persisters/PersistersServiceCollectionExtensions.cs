using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nest;
using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Persisters
{
    public static class PersistersServiceCollectionExtensions
    {
        /// <summary>
        /// Adds registration to add a <see cref="QueuePersister"/>. The persister is consumed by a hosted service implementation, <see cref="QueueProcessorHostedService"/>.
        /// Requires a service registration for <see cref="Nest.IElasticClient"/>.
        /// </summary>        
        /// <returns></returns>
        public static IServiceCollection AddQueuePersister(this IServiceCollection services, Func<IElasticClient> configureClient, Action<QueueProcessorOptions> options = null)
        {
            // registere elastic client
            services.TryAddSingleton<ElasticClientFactory>();
            services.Configure<ElasticClientFactoryOptions>(Common.ElasticClientName, options => options.ElasticClientAction = configureClient);

            // register persister
            services.AddSingleton<QueuePersister>();
            services.AddSingleton<ILogItemPersister, QueuePersister>(s => s.GetRequiredService<QueuePersister>());

            // register consumer of persister (a hosted services) and its options
            var optionsBuilder = services.AddOptions<QueueProcessorOptions>().ValidateDataAnnotations();
            if (options != null)
            { 
                optionsBuilder.Configure(options);
            }
            services.AddHostedService<QueueProcessorHostedService>();

            return services;
        }
    }
}
