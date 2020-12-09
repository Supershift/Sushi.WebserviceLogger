using Microsoft.Extensions.Hosting;
using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Persisters
{
    public class QueuePersister : ILogItemPersister 
    {
        public QueuePersister(ElasticConfiguration elasticConfiguration)
        {
            Configuration = elasticConfiguration;
        }

        public ElasticConfiguration Configuration { get; private set; }

        private ConcurrentQueue<QueuedItem> operations = new ConcurrentQueue<QueuedItem>();
        private static readonly ConcurrentDictionary<string, object> IndexCache = new ConcurrentDictionary<string, object>();

        public Task StoreLogItemAsync<T>(T logItem, string index) where T : LogItem
        {
            StoreLogItem(logItem, index);
            return Task.CompletedTask;
        }

        public void StoreLogItem<T>(T logItem, string index) where T : LogItem
        {
            if (logItem == null)
            {
                throw new ArgumentNullException(nameof(logItem));
            }

            //create new operation for bulk insertion            
            var operation = new Nest.BulkIndexOperation<T>(logItem);
            operation.Index = index;
            var queueItem = new QueuedItem()
            {
                Operation = operation
            };
            //checking of index needs to be done through a delegate, because NEST currently does not support a non-generic way of putting(updating) an index mapping            
            queueItem.CheckIfIndexExistsDelegate = () =>
            {
                //create elastic client            
                var elasticClient = ElasticClientFactory.CreateClient(Configuration);
                ElasticUtility.CreateIndexIfNotExists<T>(elasticClient, index);
            };
            operations.Enqueue(queueItem);            
        }

        public QueuedItem Dequeue()
        {
            operations.TryDequeue(out var operation);
            return operation;
        }
    }

    public class QueuedItem
    {
        public Nest.IBulkOperation Operation { get; set; }        
        public Action CheckIfIndexExistsDelegate { get; set; }
    }

    
}
