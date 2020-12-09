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
            
                //apply settings
                string elasticUrl = ConfigurationManager.AppSettings["elasticUrl"];
                string elasticUser = ConfigurationManager.AppSettings["elasticUsername"];
                string elasticPassword = ConfigurationManager.AppSettings["elasticPassword"];
                Config = new Core.ElasticConfiguration(elasticUrl, elasticUser, elasticPassword);
            
            
        }

        public static Core.ElasticConfiguration Config { get; private set; }
    }
}
