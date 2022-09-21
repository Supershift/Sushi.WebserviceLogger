using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Test
{
    [TestClass]
    public class Initialization
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {

            var config = new ConfigurationBuilder()
            .SetBasePath(context.TestDir)
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets(System.Reflection.Assembly.GetExecutingAssembly(), optional: true)
            .Build();


            //apply settings            
            var elasticSettings = new Nest.ConnectionSettings(new Elasticsearch.Net.InMemoryConnection());
            ElasticClient = new Nest.ElasticClient(elasticSettings);
            Persister = new InProcessPersister(ElasticClient);
        }

        
        public static Nest.IElasticClient ElasticClient { get; private set; }
        public static ILogItemPersister Persister { get; private set; }
    }
}
