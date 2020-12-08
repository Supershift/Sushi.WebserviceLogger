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
            queueItem.CheckIfIndexExistsDelegate = async () =>
            {
                //create elastic client            
                var elasticClient = ElasticClientFactory.CreateClient(Configuration);
                await ElasticUtility.CreateIndexIfNotExistsAsync<T>(elasticClient, index);
            };
            operations.Enqueue(queueItem);
            return Task.CompletedTask;
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
        public Func<Task> CheckIfIndexExistsDelegate { get; set; }
    }

    //https://docs.microsoft.com/th-th/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-2.1&tabs=visual-studio
    public class QueueProcessorHostedService : BackgroundService
    {
        public QueueProcessorHostedService(QueuePersister persister)
        {
            Persister = persister;            
        }

        public int MaxBulkSize { get; set; } = 100;
        public int WaitTime { get; set; } = 1000;
        public QueuePersister Persister { get; set; }
        
        public int MaxBatchSize { get; set; }
        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            int processedItems = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    processedItems = await ProcessQueueAsync();
                    if (processedItems == 0)
                        await Task.Delay(WaitTime);
                }
                catch
                {
                    //todo: some sort of callback to allow clients to log transactions?
                }
            }
            
            do
            {
                //get remaining items and save
                processedItems = await ProcessQueueAsync();
            }
            while(processedItems > 0);
        }

        public async Task<int> ProcessQueueAsync()
        {
            var bulkDescriptor = new Nest.BulkDescriptor();            
            
            int itemCount = 0;
            for (int i = 0; i < MaxBulkSize; i++)
            {
                //get item from queue                
                var item = Persister.Dequeue();

                if (item != null)
                {
                    //check if we need to create the index
                    if (item.CheckIfIndexExistsDelegate != null)
                        await item.CheckIfIndexExistsDelegate();
                    //add operation to bulk operation
                    bulkDescriptor.AddOperation(item.Operation);                    
                    itemCount++;
                }
                else
                    break;
            }
            if (itemCount > 0)
            {
                var client = Core.ElasticClientFactory.CreateClient(Persister.Configuration);
                var result = await client.BulkAsync(bulkDescriptor);
            }
            return itemCount;
        }
    }
}
