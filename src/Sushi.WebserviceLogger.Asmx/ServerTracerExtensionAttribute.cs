//using Sushi.WebserviceLogger.Core;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Web.Services.Protocols;

//namespace Sushi.WebserviceLogger.Asmx
//{
//    [AttributeUsage(AttributeTargets.Method)]
//    public class ServerTracerExtensionAttribute : SoapExtensionAttribute
//    {
//        private int priority;

//        public override Type ExtensionType
//        {
//            get { return typeof(ServerTracer<LogItem>); }
//        }

//        public override int Priority
//        {
//            get { return priority; }
//            set { priority = value; }
//        }

//        public ILogItemPersister Persister { get; set; }
//    }
//}
