using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sushi.WebserviceLogger.Core;
using Sushi.WebserviceLogger.Filter;
using Sushi.WebserviceLogger.Persisters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Test
{
    [TestClass]
    public class RegisterFilterTest
    {
        [TestMethod]
        public void RegisterFilter()
        {
            // add services
            var services = new ServiceCollection();

            // add persister
            services.AddMockPersister();

            // add an http client for MyConnector, which uses webservicelogging
            services.AddWebserviceLoggerFilter<LogItem>(b => { });

            // build provider
            var provider = services.BuildServiceProvider();

            // get filter instance
            var myConnector = provider.GetRequiredService<WebserviceLoggerFilter<LogItem>>();
        }
    }
}
