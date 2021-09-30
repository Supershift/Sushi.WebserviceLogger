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
    public class RetrieveItems
    {
        string indexName = "webservicelogs_test";

        [TestMethod]
        public async Task RetrieveAll()
        {
            var client = ElasticClientFactory.CreateClient(Initialization.ElasticConfig);
            var result = await client.SearchAsync<LogItem>(s => s.Index(indexName).MatchAll());
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented));
        }
    }
}
