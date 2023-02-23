using Microsoft.Extensions.DependencyInjection;
using Sushi.Elastic.ClientFactory;
using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Persisters
{
    /// <summary>
    /// Represents a persister which immediately writes log items to Elastic.
    /// </summary>
    public class InProcessPersister : ILogItemPersister
    {
        /// <summary>
        /// Creates a new instance of <see cref="InProcessPersister"/>.
        /// </summary>        
        [ActivatorUtilitiesConstructor]
        public InProcessPersister(ElasticClientFactory elasticClientFactory)
        {
            _elasticClient = elasticClientFactory.GetClient(Common.ElasticClientName);
        }

        /// <summary>
        /// Creates a new instance of <see cref="InProcessPersister"/>.
        /// </summary>        
        public InProcessPersister(Nest.IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        private Nest.IElasticClient _elasticClient;

        /// <summary>
        /// Stores a <see cref="LogItem"/> in Elastic.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logItem"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task StoreLogItemAsync<T>(T logItem, string index) where T : LogItem
        {
            //index logItem                
            var response = await _elasticClient.CreateAsync(logItem, i => i.Index(index));
        }
    }
}
