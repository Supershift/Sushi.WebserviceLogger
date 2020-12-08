using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Core
{
    /// <summary>
    /// Represents a persister which immediately writes log items to Elastic.
    /// </summary>
    public class InProcessPersister : ILogItemPersister
    {
        /// <summary>
        /// Gets the configuration for Elastic used by this instance.
        /// </summary>
        public ElasticConfiguration Configuration { get; private set; }

        /// <summary>
        /// Creates a new instance of <see cref="InProcessPersister"/>.
        /// </summary>
        /// <param name="configuration"></param>
        public InProcessPersister(ElasticConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Stores a <see cref="LogItem"/> in Elastic.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logItem"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task StoreLogItemAsync<T>(T logItem, string index) where T : LogItem
        {
            //create elastic client            
            var elasticClient = ElasticClientFactory.CreateClient(Configuration);

            //create index with mapping if not exists                
            await ElasticUtility.CreateIndexIfNotExistsAsync<T>(elasticClient, index);

            //index logItem                
            var response = await elasticClient.IndexAsync(logItem, i => i.Index(index));
        }
    }
}
