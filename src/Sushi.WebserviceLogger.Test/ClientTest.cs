using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Test
{
    [TestClass]
    public class ClientTest
    {
        public static readonly System.Net.Http.HttpClient Client;

        static ClientTest()
        {
            var logger = new Logger<LogItem>(Initialization.Persister, new LoggerOptions<LogItem>());
            var handler = MessageHandler.CreateHttpClientMessageHandler<LogItem>(logger);
            Client = new System.Net.Http.HttpClient(handler);            
        }

        public string ServiceBaseUrl = "http://localhost/Sushi.WebserviceLogger.SampleService/api/";

        [TestMethod]
        public async Task LogClientGetTest()
        {
            string url = ServiceBaseUrl + "ping";
            using (var response = await Client.GetAsync(url))
            {
                Console.WriteLine(response.StatusCode);
                Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            }
        }

        public class OrderRequest
        {
            public int Quantity { get; set; }
            public string ProductID { get; set; }
            public string ClientTransactionID { get; set; }
        }

        public class OrderResponse
        {
            public int Quantity { get; set; }
            public string ProductID { get; set; }
            public string ClientTransactionID { get; set; }
            public Guid OrderID { get; set; }
            public decimal Amount { get; set; }
        }

        [TestMethod]
        public async Task LogClientPostTest()
        {
            string url = ServiceBaseUrl + "order";

            var orderRequest = new OrderRequest()
            {
                ClientTransactionID = Guid.NewGuid().ToString(),
                ProductID = "acd-124",
                Quantity = 2
            };

            using (var requestContent = new System.Net.Http.StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(orderRequest), Encoding.UTF8, "application/json"))
            using (var response = await Client.PostAsync(url, requestContent))
            {
                Console.WriteLine(response.StatusCode);
                Console.WriteLine(await response.Content.ReadAsStringAsync());
                Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            }
        }
    }
}
