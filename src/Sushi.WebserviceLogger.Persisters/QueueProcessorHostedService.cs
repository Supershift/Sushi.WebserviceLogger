using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Persisters
{
    /// <summary>
    /// Implements a <see cref="IHostedService"/> to write queued <see cref="Core.LogItem"/> instances to Elastic. This is the preferred method to
    /// process queued items in .NET Core / .NET 5.
    /// </summary>
    public class QueueProcessorHostedService : BackgroundService
    {
        public QueueProcessorHostedService(QueuePersister persister)
        {
            Persister = persister;
        }

        public TimeSpan WaitTime { get; set; } = TimeSpan.FromSeconds(1);
        public QueuePersister Persister { get; set; }
        public int MaxBatchSize { get; set; } = 100;
        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            int processedItems = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    processedItems = await ProcessQueueAsync();
                    if (processedItems == 0)
                        await Task.Delay(WaitTime).ConfigureAwait(false);
                }
                catch
                {
                    //todo: some sort of callback to allow clients to log transactions?
                }
            }

            do
            {
                //get remaining items and save
                processedItems = await ProcessQueueAsync().ConfigureAwait(false); 
            }
            while (processedItems > 0);
        }

        public async Task<int> ProcessQueueAsync()
        {
            var bulkDescriptor = new Nest.BulkDescriptor();

            int itemCount = 0;
            for (int i = 0; i < MaxBatchSize; i++)
            {
                //get item from queue                
                var item = Persister.Dequeue();

                if (item != null)
                {
                    //check if we need to create the index
                    item.CheckIfIndexExistsDelegate?.Invoke();
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
                await client.BulkAsync(bulkDescriptor).ConfigureAwait(false); 
            }
            return itemCount;
        }
    }
}
