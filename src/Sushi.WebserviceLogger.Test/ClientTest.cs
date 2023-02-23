using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sushi.WebserviceLogger.Core;
using Sushi.WebserviceLogger.Persisters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Test
{
    [TestClass]
    public class ClientTest : TestBase
    {
        private string _name = "my_client_test";
        private ServiceProvider _serviceProvider;
        private HttpClient GetClient()
        {
            var factory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
            return factory.CreateClient(_name);
        }

        private readonly List<LogItem> _logItems = new List<LogItem>();

        public ClientTest()
        {
            // add services
            var services = new ServiceCollection();

            services.AddMockPersister();
            services
                .AddHttpClient(_name, client =>
                {
                    
                })
                .AddWebserviceLogging<LogItem>(o =>
                {
                    o.AddLogItemCallback = (logItem, c) => { _logItems.Add(logItem); return logItem; };
                });            

            // build provider
            _serviceProvider = services.BuildServiceProvider();
        }

        [TestMethod]
        public async Task LogClientGetTest()
        {
            // call url
            var client = GetClient();
            string url = "https://www.google.com";
            using var response = await client.GetAsync(url);
            
            // get logged item
            var logItem = _logItems.FirstOrDefault();

            WriteResult(logItem);
            Console.WriteLine(response.StatusCode);
            
            Assert.IsNotNull(logItem);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);            
        }

        
    }
}
