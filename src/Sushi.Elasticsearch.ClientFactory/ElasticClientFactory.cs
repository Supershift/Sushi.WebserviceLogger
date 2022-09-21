using Microsoft.Extensions.Options;
using Nest;
using System.Collections.Concurrent;

namespace Sushi.Elastic.ClientFactory
{
    /// <summary>
    /// Factory to register instances of <see cref="IElasticClient"/> with different configuration.
    /// </summary>
    public class ElasticClientFactory
    {
        private readonly ConcurrentDictionary<string, IElasticClient> _clients;
        private readonly IOptionsMonitor<ElasticClientFactoryOptions> _options;

        /// <summary>
        /// Creates a new instance of <see cref="ElasticClientFactory"/>.
        /// </summary>
        /// <param name="options"></param>
        public ElasticClientFactory(IOptionsMonitor<ElasticClientFactoryOptions> options)
        {
            _clients = new ConcurrentDictionary<string, IElasticClient>();
            _options = options;
        }

        /// <summary>
        /// Gets a <see cref="IElasticClient"/> registered with <paramref name="name"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public IElasticClient GetClient(string name)
        {
            // get the client for the given name. if it does not exist, create one.
            var client = _clients.GetOrAdd(name, name =>
            {
                var options = _options.Get(name);
                if (options == null)
                {
                    throw new Exception($"No ElasticClient registered for {name}");
                }
                var client = options.CreateElasticClient();
                return client;
            });

            return client;
        }
    }
}
