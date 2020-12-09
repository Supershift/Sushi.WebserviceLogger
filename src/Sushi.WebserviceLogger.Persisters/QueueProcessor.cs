using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Persisters
{
    public class QueueProcessor
    {
        public QueueProcessor(QueuePersister persister)
        {
            Persister = persister;
        }

        public TimeSpan WaitTime { get; set; } = TimeSpan.FromSeconds(1);
        public QueuePersister Persister { get; set; }
        public int MaxBatchSize { get; set; } = 100;

        public void Execute(CancellationToken cancellationToken)
        {
            int processedItems = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    processedItems = ProcessQueue();
                    if (processedItems == 0)
                        Thread.Sleep(WaitTime);
                }
                catch
                {
                    //todo: some sort of callback to allow clients to log transactions?
                }
            }

            do
            {
                //get remaining items and save
                processedItems = ProcessQueue();
            }
            while (processedItems > 0);
        }

        public int ProcessQueue()
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
                client.Bulk(bulkDescriptor);
            }
            return itemCount;
        }
    }
}
