using Microsoft.Extensions.Options;
using Nest;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Core
{
    public class ElasticClientFactoryOptions
    {
        public Func<IElasticClient> ElasticClientAction { get; set; }
    }
    
    public class ElasticClientFactory
    {
        private readonly ConcurrentDictionary<string, IElasticClient> _clients;
        private readonly IOptionsMonitor<ElasticClientFactoryOptions> _options;

        public ElasticClientFactory(IOptionsMonitor<ElasticClientFactoryOptions> options)
        {
            _clients = new ConcurrentDictionary<string, IElasticClient>();
            _options = options;
        }
        
        public IElasticClient GetClient(string name)
        {
            var client = _clients.GetOrAdd(name, name =>
            {
                var options = _options.Get(name);
                if(options == null)
                {
                    throw new Exception($"No ElasticClient registered for {name}");
                }
                var client = options.ElasticClientAction();
                return client;
            });
            
            return client;
        }
    }
}
