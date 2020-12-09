using Sushi.WebserviceLogger.Asmx;
using Sushi.WebserviceLogger.Core;
using Sushi.WebserviceLogger.Persisters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

            //apply settings
            string elasticUrl = ConfigurationManager.AppSettings["elasticUrl"];
            string elasticUsername = ConfigurationManager.AppSettings["elasticUsername"];
            string elasticPassword = ConfigurationManager.AppSettings["elasticPassword"];            
            var loggingConfig = new Core.ElasticConfiguration(elasticUrl, elasticUsername, elasticPassword);

            //create handler with logging
            var handler = Core.MessageHandler.CreateWebApiMessageHandler<LogItem>(loggingConfig);

            //add handler to pipeline
            config.MessageHandlers.Add(handler);

            //create asmx persister and logger
            //var persister = new InProcessPersister(loggingConfig);
            var persister = new QueuePersister(loggingConfig);
            Config.AsmxLogger = new Logger<LogItem>(persister)
            {
                IndexNameCallback = (dt) => $"asmxlogs_{dt:yyyy-MM}"
            };

            //create a background worker to store queued logitems into elastic
            var backgroundworker = new QueueProcessor(persister);
            backgroundworker.Start(Global.CancellationTokenSource.Token);
        }
    }
}