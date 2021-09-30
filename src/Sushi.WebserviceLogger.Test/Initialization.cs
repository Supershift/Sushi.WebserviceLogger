using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            string elasticUrl = config["elasticUrl"];
            string elasticUser = config["elasticUsername"];
            string elasticPassword = config["elasticPassword"];
            ElasticConfig = new Core.ElasticConfiguration(elasticUrl, elasticUser, elasticPassword);
            LoggerConfig = new Core.LoggerConfiguration(new Core.InProcessPersister(ElasticConfig));

        }

        public static Core.ElasticConfiguration ElasticConfig { get; private set; }
        public static Core.LoggerConfiguration LoggerConfig { get; private set; }
    }
}
