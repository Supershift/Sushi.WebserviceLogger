using Microsoft.AspNetCore.Http;
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
    public class LoggerTest
    {
        public class MyLogItem : Core.LogItem
        {
            public string ProductID { get; set; }
        }

        public class MyDemoRequest
        {            
            public int Quantity { get; set; }
            public string ProductID { get; set; }
        }

       
        

        [TestMethod]
        public async Task AddLogItemTest()
        {
            var myDemoRequest = new MyDemoRequest() { ProductID = "ABC-1234", Quantity = 2 };

            var options = new LoggerOptions()
            {
                IndexNameCallback = () => "webservicelogs-test"
            };
            var logger = new Logger(Initialization.Persister, options);

            var requestData = new Core.RequestData()
            {
                Body = new Core.Body()
                {
                    ContentType = Core.ContentType.JSON,
                    Data = Newtonsoft.Json.JsonConvert.SerializeObject(myDemoRequest)

                },
                ClientIP = "127.0.0.1",
                Headers = new List<Header>() { new Header("Content-Type", "application/json"), new Core.Header("x-mycustomheader", "my value") },
                Started = new DateTime(2019,01,01).Add(DateTime.UtcNow.TimeOfDay),
                Url = Utility.GetUrlFromUri(new Uri("http://localhost/myService"))
            };
            var responseData = new Core.ResponseData()
            {
                Body = new Core.Body()
                {
                    ContentType = Core.ContentType.Raw,
                    Data = "OK"
                },
                HttpStatusCode = 200,
                Started = requestData.Started.AddMilliseconds(500),
                Headers = new List<Core.Header>() { new Core.Header("Content-Type", "application/json") }
            };

            await logger.AddLogItemAsync(requestData, responseData, Core.ContextType.Server);
        }

        

        [TestMethod]
        public async Task AddGenericLogItemTest()
        {
            var myDemoRequest = new MyDemoRequest() { ProductID = "ABC-1234", Quantity = 2 };

            var options = new LoggerOptions<MyLogItem>()
            {
                AddLogItemCallback = MyDelegate
            };
            var logger = new Core.Logger<MyLogItem>(Initialization.Persister, options);            

            var requestData = new Core.RequestData()
            {
                Body = new Core.Body()
                {
                    ContentType = Core.ContentType.JSON,
                    Data = Newtonsoft.Json.JsonConvert.SerializeObject(myDemoRequest)

                },
                ClientIP = "127.0.0.1",
                Headers = new List<Core.Header>() { new Core.Header("Content-Type", "application/json"), new Core.Header("x-mycustomheader", "my value") },
                Started = DateTime.UtcNow.AddMinutes(-2),
                Url = Utility.GetUrlFromUri(new Uri("http://localhost/myService"))
            };
            var responseData = new Core.ResponseData()
            {
                Body = new Core.Body()
                {
                    ContentType = Core.ContentType.Raw,
                    Data = "OK"
                },
                HttpStatusCode = 200,
                Started = DateTime.UtcNow.AddMinutes(-1),
                Headers = new List<Core.Header>() { new Core.Header("Content-Type", "application/json") }
            };

            await logger.AddLogItemAsync(requestData, responseData, Core.ContextType.Server);
        }

        [TestMethod]
        public async Task AddCorrelationIdTest()
        {
            var myDemoRequest = new MyDemoRequest() { ProductID = "ABC-1234", Quantity = 2 };

            string correlationID= Guid.NewGuid().ToString();
            var options = new LoggerOptions<MyLogItem>()
            {
                AddLogItemCallback = MyDelegate,
                CorrelationIdCallback = (c) => { return correlationID; }
            };
            var logger = new Logger<MyLogItem>(Initialization.Persister, options);
            
            var requestData = new Core.RequestData()
            {
                Body = new Core.Body()
                {
                    ContentType = Core.ContentType.JSON,
                    Data = Newtonsoft.Json.JsonConvert.SerializeObject(myDemoRequest)

                },
                ClientIP = "127.0.0.1",
                Headers = new List<Core.Header>() { new Core.Header("Content-Type", "application/json"), new Core.Header("x-mycustomheader", "my value") },
                Started = DateTime.UtcNow.AddMinutes(-2),
                Url = Utility.GetUrlFromUri(new Uri("http://localhost/myService"))
            };
            var responseData = new Core.ResponseData()
            {
                Body = new Core.Body()
                {
                    ContentType = Core.ContentType.Raw,
                    Data = "OK"
                },
                HttpStatusCode = 200,
                Started = DateTime.UtcNow.AddMinutes(-1),
                Headers = new List<Core.Header>() { new Core.Header("Content-Type", "application/json") }
            };

            var result = await logger.AddLogItemAsync(requestData, responseData, ContextType.Server);

            Assert.AreEqual(result.CorrelationID, correlationID);
        }

        [TestMethod]
        public async Task MulticastLogItemCallbackWithBlockTest()
        {
            var myDemoRequest = new MyDemoRequest() { ProductID = "ABC-1234", Quantity = 2 };

            string correlationID = Guid.NewGuid().ToString();
            var options = new LoggerOptions<MyLogItem>()
            {
                ExceptionCallback = this.ExceptionCallback
        };
            options.AddLogItemCallback += Block;
            options.AddLogItemCallback += MyDelegate;
            var logger = new Logger<MyLogItem>(Initialization.Persister, options);
          
            var requestData = new Core.RequestData()
            {
                Body = new Core.Body()
                {
                    ContentType = Core.ContentType.JSON,
                    Data = Newtonsoft.Json.JsonConvert.SerializeObject(myDemoRequest)

                },
                ClientIP = "127.0.0.1",
                Headers = new List<Core.Header>() { new Core.Header("Content-Type", "application/json"), new Core.Header("x-mycustomheader", "my value") },
                Started = DateTime.UtcNow.AddMinutes(-2),
                Url = Utility.GetUrlFromUri(new Uri("http://localhost/myService"))
            };
            var responseData = new Core.ResponseData()
            {
                Body = new Core.Body()
                {
                    ContentType = Core.ContentType.Raw,
                    Data = "OK"
                },
                HttpStatusCode = 200,
                Started = DateTime.UtcNow.AddMinutes(-1),
                Headers = new List<Core.Header>() { new Core.Header("Content-Type", "application/json") }
            };

            var result = await logger.AddLogItemAsync(requestData, responseData, ContextType.Server);

            Assert.IsNull(result);
        }

        public MyLogItem MyDelegate(MyLogItem logItem, HttpContext context)
        {
            logItem.ProductID = "test";
            return logItem;
        }

        public MyLogItem Block(MyLogItem logItem, HttpContext context)
        {
            return null;
        }

        public bool ExceptionCallback(Exception exception, MyLogItem logItem, HttpContext context)
        {
            // log the exception
            Console.WriteLine(exception);
            
            // do not throw the exception
            return false;
        }
    }
}
