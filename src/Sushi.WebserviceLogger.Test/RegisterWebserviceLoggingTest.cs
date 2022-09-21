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
    public class RegisterWebserviceLoggingTest
    {
        public class MyConnector
        {
            public static readonly string Name = "a connector name";
            private readonly HttpClient _client;

            public MyConnector(IHttpClientFactory httpClientFactory)
            {
                _client = httpClientFactory.CreateClient(Name);
            }
        }

        [TestMethod]
        public void RegisterMessageHandlerByType()
        {
            // add services
            var services = new ServiceCollection();

            // add persister
            services.AddMockPersister();
            
            // add an http client for MyConnector, which uses webservicelogging
            services
                .AddHttpClient<MyConnector>(client =>
                {
                    client.BaseAddress = new Uri("https://www.google.com");
                })
                .AddWebserviceLogging<LogItem>(o =>
                {
                    o.MaxBodyContentLength = 17000;
                });

            // add my connector
            services.AddTransient<MyConnector>();

            // build provider
            var provider = services.BuildServiceProvider();

            // get my connector instance
            var myConnector = provider.GetRequiredService<MyConnector>();

            
        }

        [TestMethod]
        public void RegisterMessageHandlerByName()
        {
            // add services
            var services = new ServiceCollection();

            services.AddMockPersister();
            services
                .AddHttpClient(MyConnector.Name, client =>
                {
                    client.BaseAddress = new Uri("https://www.google.com");
                })
                .AddWebserviceLogging<LogItem>(o =>
                {
                    o.MaxBodyContentLength = 17000;
                });

            services.AddTransient<MyConnector>();

            // build provider
            var provider = services.BuildServiceProvider();

            // get my connector instance
            var myConnector = provider.GetRequiredService<MyConnector>();
        }
    }
}
