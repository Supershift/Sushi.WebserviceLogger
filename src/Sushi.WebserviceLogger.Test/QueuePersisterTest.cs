using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sushi.WebserviceLogger.Core;
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
        private ServiceProvider _serviceProvider;
        private readonly List<LogItem> _logItems = new List<LogItem>();

        public QueuePersisterTest()
        {
            // add services
            var services = new ServiceCollection();

            var connection = new Elasticsearch.Net.InMemoryConnection();
            var elasticClient = new Nest.ElasticClient(new Nest.ConnectionSettings(connection));
            services.AddQueuePersister(elasticClient);            
            
            // build provider
            _serviceProvider = services.BuildServiceProvider();
        }

        [TestMethod]
        public async Task IndexItems()
        {
            var queuePersister = _serviceProvider.GetRequiredService<QueuePersister>();
            var hostedServices = _serviceProvider.GetServices<IHostedService>();
            var processor = hostedServices.FirstOrDefault(x=> x is QueueProcessorHostedService) as QueueProcessorHostedService;

            int itemCount = 100;
            
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
            
            int itemsProcessed = await processor.ProcessQueueAsync();
            
            Console.WriteLine(itemsProcessed);
            Assert.AreEqual(itemsProcessed, itemCount);
        }
    }
}
