using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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
            //open secrets file
            using (var fs = File.OpenRead("secrets.json"))
            using (var sr = new StreamReader(fs))
            {
                //read and deserialize secrets file
                var fileData = sr.ReadToEnd();
                var secrets = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(fileData);

                //apply settings
                string elasticUrl = secrets["elasticUrl"];
                string elasticUser = secrets["elasticUsername"];
                string elasticPassword = secrets["elasticPassword"];
                Config = new Core.ElasticConfiguration(elasticUrl, elasticUser, elasticPassword);
            }
            
        }

        public static Core.ElasticConfiguration Config { get; private set; }
    }
}
