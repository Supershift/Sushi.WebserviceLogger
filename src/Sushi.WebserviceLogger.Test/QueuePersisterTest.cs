using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sushi.WebserviceLogger.Persisters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Sushi.WebserviceLogger.Test.LoggerTest;

namespace Sushi.WebserviceLogger.Test
{
    [TestClass]
    public class QueuePersisterTest
    {
        [TestMethod]
        public async Task IndexItems()
        {

            var queuePersister = new QueuePersister();
            int itemCount = 100;
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < itemCount; i++)
            {
                var myLogItem = new MyLogItem()
                {
                    Id = Guid.NewGuid().ToString(),
                    ProductID = Guid.NewGuid().ToString(),                    
                    Timestamp = DateTime.UtcNow

                };
                await queuePersister.StoreLogItemAsync(myLogItem, "webservicelogs-test");
            }
            Console.WriteLine($"Adding to queue: {sw.Elapsed}");

            //process items
            sw.Restart();
            var processor = new QueueProcessorHostedService(queuePersister, Initialization.ElasticClient);
            processor.MaxBatchSize = itemCount;
            int itemsProcessed = await processor.ProcessQueueAsync();
            Console.WriteLine($"Persisting queue: {sw.Elapsed}");
            Console.WriteLine(itemsProcessed);            
            Assert.AreEqual(itemsProcessed, processor.MaxBatchSize);
        }
    }
}
