﻿using Microsoft.Extensions.Hosting;
using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Persisters
{
    /// <summary>
    /// Represents a persister which writes logitems to a <see cref="ConcurrentQueue{T}"/>.
    /// An additional method is needed to retrieve the items and write them to Elastic. The preferred option is <see cref="QueueProcessorHostedService"/>.
    /// </summary>
    public class QueuePersister : ILogItemPersister 
    {
        /// <summary>
        /// Creates a new instance of <see cref="QueuePersister"/>.
        /// </summary>
        /// <param name="elasticConfiguration"></param>
        public QueuePersister(ElasticConfiguration elasticConfiguration)
        {
            Configuration = elasticConfiguration;
        }

        /// <summary>
        /// Gets the configuration used to write to Elastic by the consumer of the queue.
        /// </summary>
        public ElasticConfiguration Configuration { get; private set; }

        private ConcurrentQueue<QueuedItem> operations = new ConcurrentQueue<QueuedItem>();
        private static readonly ConcurrentDictionary<string, object> IndexCache = new ConcurrentDictionary<string, object>();

        /// <summary>
        /// Adds a <see cref="LogItem"/> to the queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logItem"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public Task StoreLogItemAsync<T>(T logItem, string index) where T : LogItem
        {
            StoreLogItem(logItem, index);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Adds a <see cref="LogItem"/> to the queue.
        /// </summary>
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

        /// <summary>
        /// Dequeues one item from the queue. If the queue is empty, NULL is returned.
        /// </summary>
        /// <returns></returns>
        public QueuedItem Dequeue()
        {
            operations.TryDequeue(out var operation);
            return operation;
        }
    }

    /// <summary>
    /// Represents an item in the queue.
    /// </summary>
    public class QueuedItem
    {
        /// <summary>
        /// Gets or sets the operation to perform to Elastic for this item.
        /// </summary>
        public Nest.IBulkOperation Operation { get; set; }        
        /// <summary>
        /// Gets or sets the delegate to create an index for this item.
        /// </summary>
        public Action CheckIfIndexExistsDelegate { get; set; }
    }

    
}
