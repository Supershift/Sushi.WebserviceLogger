using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.Elastic.ClientFactory
{
    /// <summary>
    /// Extension methods to configure a <see cref="IServiceCollection"/> for <see cref="ElasticClientFactory"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers <see cref="ElasticClientFactory"/> and adds a binding between <paramref name="name"/> and <paramref name="createClient"/> to the factory.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="name"></param>
        /// <param name="createClient"></param>
        /// <returns></returns>
        public static IServiceCollection AddElasticClient(this IServiceCollection services, string name, Func<IElasticClient> createClient)
        {
            services.TryAddSingleton<ElasticClientFactory>();
            services.Configure<ElasticClientFactoryOptions>(name, options => options.CreateElasticClient = createClient);

            return services;
        }

        /// <summary>
        /// Registers <see cref="ElasticClientFactory"/> and adds a binding between <paramref name="name"/> and <paramref name="client"/> to the factory.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="name"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public static IServiceCollection AddElasticClient(this IServiceCollection services, string name, IElasticClient client)
        {
            return services.AddElasticClient(name, () => client);
        }
    }
}
