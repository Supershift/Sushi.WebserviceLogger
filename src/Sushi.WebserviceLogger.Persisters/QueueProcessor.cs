using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Persisters
{
    /// <summary>
    /// Represents a legacy <see cref="QueueProcessor"/> which can be used on .NET Framework to process logitems inserted by a <see cref="QueuePersister"/>.
    /// </summary>
    public class QueueProcessor
    {
        /// <summary>
        /// Creates a new instance of <see cref="QueueProcessor"/> which can be used to process logitems for the provided <paramref name="persister"/>.
        /// </summary>
        /// <param name="persister"></param>
        public QueueProcessor(QueuePersister persister)
        {
            Persister = persister;
        }

        /// <summary>
        /// Gets or sets a value indicating how much time is spent waiting if the persister's buffer is empty.
        /// </summary>
        public TimeSpan WaitTime { get; set; } = TimeSpan.FromSeconds(1);
        /// <summary>
        /// Gets the persister associated with this processor.
        /// </summary>
        public QueuePersister Persister { get; private set; }
        /// <summary>
        /// Gets or sets the maximum number of items inserted into Elastic in one bulk operation.
        /// </summary>
        public int MaxBatchSize { get; set; } = 100;

        /// <summary>
        /// Gets or sets a delegate which is called when an <see cref="Exception"/> is encountered while performing <see cref="Execute(CancellationToken)"/>.
        /// </summary>
        public Action<Exception> ExceptionDelegate { get; set; }

        /// <summary>
        /// Starts a new long running <see cref="Task"/> for the <see cref="QueueProcessor"/>.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Start(CancellationToken cancellationToken)
        {
            var result = Task.Factory.StartNew(() => this.Execute(cancellationToken), TaskCreationOptions.LongRunning);
            return result;
        }

        /// <summary>
        /// Continously writes items from a <see cref="QueuePersister"/> to Elastic until cancellation is requested.
        /// </summary>
        /// <param name="cancellationToken"></param>
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
                catch(Exception ex)
                {
                    try
                    {
                        ExceptionDelegate?.Invoke(ex);
                    }
                    catch { }
                }
            }

            do
            {
                //get remaining items and save
                processedItems = ProcessQueue();
            }
            while (processedItems > 0);
        }

        /// <summary>
        /// Takes a number of items from a <see cref="QueuePersister"/> and inserts them into Elastic. The maximum number of items is determined by <see cref="MaxBatchSize"/>.
        /// </summary>
        /// <returns></returns>
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
