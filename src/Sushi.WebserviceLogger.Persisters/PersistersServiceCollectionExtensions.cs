using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nest;
using Sushi.Elastic.ClientFactory;
using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Persisters
{
    /// <summary>
    /// Extension methods to configure a <see cref="IServiceCollection"/> for <see cref="ILogItemPersister"/> implementations.
    /// </summary>
    public static class PersistersServiceCollectionExtensions
    {
        /// <summary>
        /// Registers dependencies for a <see cref="QueuePersister"/>. The persister is consumed by a hosted service implementation, <see cref="QueueProcessorHostedService"/>.        
        /// </summary>        
        /// <returns></returns>
        public static IServiceCollection AddQueuePersister(this IServiceCollection services, Func<IElasticClient> createClient, Action<QueueProcessorOptions> options = null)
        {
            // register elastic client
            services.AddElasticClient(Common.ElasticClientName, createClient);

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

        /// <summary>
        /// Registers a <see cref="MockPersister"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMockPersister(this IServiceCollection services)
        {
            services.AddTransient<ILogItemPersister, MockPersister>();
            return services;
        }
    }
}
