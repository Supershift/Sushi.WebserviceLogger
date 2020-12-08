using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Sushi.WebserviceLogger.SampleService
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();

            //add custom message handler for logging

            //open secrets file
            using (var fs = File.OpenRead("secrets.json"))
            using (var sr = new StreamReader(fs))
            {
                //read and deserialize secrets file
                var fileData = sr.ReadToEnd();
                var secrets = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(fileData);

                //apply settings
                string elasticUrl = secrets["elasticUrl"];
                string elasticUsername = secrets["elasticUsername"];
                string elasticPassword = secrets["elasticPassword"];
                var loggingConfig = new Core.ElasticConfiguration(elasticUrl, elasticUsername, elasticPassword);

                //create handler with logging
                var handler = Core.MessageHandler.CreateWebApiMessageHandler<LogItem>(loggingConfig);

                //add handler to pipeline
                config.MessageHandlers.Add(handler);
            }

            
            
        }
    }
}