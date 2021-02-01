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
    public class ElasticTest
    {
        [TestMethod]
        public async Task CreateIndexIfNotExistsConcurrentTest()
        {
            var elasticClient = ElasticClientFactory.CreateClient(Initialization.Config);

            string indexName = "create-if-not-exists";
            // make sure the index does not exist
            var exists = await elasticClient.Indices.ExistsAsync(indexName);
            if (exists.Exists)
            {
                await elasticClient.Indices.DeleteAsync(indexName);
            }

            // start 2 taks concurrently
            var task1 = ElasticUtility.CreateIndexIfNotExistsAsync<LogItem>(elasticClient, indexName);            
            var task2 = ElasticUtility.CreateIndexIfNotExistsAsync<LogItem>(elasticClient, indexName);

            // await both
            await Task.WhenAll(task1, task2);

            // one should be true and the other should be false
            Assert.IsTrue((task1.Result == true && task2.Result == false) || (task1.Result == false && task2.Result == true));
        }

        
    }
}
