using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Sushi.Elastic.ClientFactory;
using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Persisters
{
    /// <summary>
    /// Implements a <see cref="IHostedService"/> to write queued <see cref="Core.LogItem"/> instances to Elastic. This is the preferred method to process queued items.
    /// </summary>
    public class QueueProcessorHostedService : BackgroundService
    {
        private Nest.IElasticClient _elasticClient;
        private readonly QueueProcessorOptions _options;
        private readonly QueuePersister _persister;

        /// <summary>
        /// Creates a new instance of <see cref="QueueProcessorHostedService"/> which can be used to process logitems for the provided <paramref name="persister"/>.
        /// </summary>        
        [ActivatorUtilitiesConstructor]
        public QueueProcessorHostedService(QueuePersister persister, ElasticClientFactory elasticClientFactory, IOptions<QueueProcessorOptions> options)
        {
            _persister = persister;
            _elasticClient = elasticClientFactory.GetClient(Common.ElasticClientName);
            _options = options.Value;
        }

        public QueueProcessorHostedService(QueuePersister persister, Nest.IElasticClient elasticClient, IOptions<QueueProcessorOptions> options)
        {
            _persister = persister;
            _elasticClient = elasticClient;
            _options = options.Value;
        }
        
        /// <summary>
        /// Continously writes items from a <see cref="QueuePersister"/> to Elastic until cancellation is requested.
        /// </summary>
        /// <param name="cancellationToken"></param>
        protected async override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            int processedItems = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    processedItems = await ProcessQueueAsync();
                    if (processedItems == 0)
                        await Task.Delay(_options.WaitTime).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    try
                    {
                        _options.ExceptionDelegate?.Invoke(ex);
                    }
                    catch { }
                }
            }

            do
            {
                //get remaining items and save
                processedItems = await ProcessQueueAsync().ConfigureAwait(false); 
            }
            while (processedItems > 0);
        }

        /// <summary>
        /// Takes a number of items from a <see cref="QueuePersister"/> and inserts them into Elastic. The maximum number of items is determined by <see cref="MaxBatchSize"/>.
        /// </summary>
        /// <returns></returns>
        public async Task<int> ProcessQueueAsync()
        {
            var bulkDescriptor = new Nest.BulkDescriptor();

            int itemCount = 0;
            for (int i = 0; i < _options.MaxBatchSize; i++)
            {
                //get item from queue                
                var item = _persister.Dequeue();

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
                await _elasticClient.BulkAsync(bulkDescriptor).ConfigureAwait(false); 
            }
            return itemCount;
        }
    }
}
