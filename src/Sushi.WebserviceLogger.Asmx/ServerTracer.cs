using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;

namespace Sushi.WebserviceLogger.Asmx
{
    public abstract class ServerTracer<T> : SoapExtension where T: LogItem, new()
    {

        public override object GetInitializer(Type serviceType)
        {
            return null;
        }

        public override object GetInitializer(LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute)
        {
            return null;
        }

        public abstract Logger<T> Logger { get; set; }

        private System.IO.Stream SoapStream;

        private RequestData RequestData { get; set; }

        public override void ProcessMessage(SoapMessage message)
        {   
            switch (message.Stage)
            {
                case SoapMessageStage.BeforeSerialize:                    
                    break;
                case SoapMessageStage.AfterSerialize:
                    ProcessOutgoingMessage(message);
                    break;
                case SoapMessageStage.BeforeDeserialize:
                    ProcessIncomingMessage(message);
                    break;
                case SoapMessageStage.AfterDeserialize:
                    //get action from message 
                    if (RequestData != null && RequestData.SoapAction == null && message?.Action != null)
                        RequestData.SoapAction = message.Action;                   
                    break;
            }
        }

        protected void ProcessIncomingMessage(SoapMessage message)
        {
            try
            {
                //copy from the incoming soap stream to a local memory stream
                SoapStream.CopyTo(message.Stream);
                message.Stream.Position = 0;

                try
                {
                    RequestData = Utility.GetRequestFromMessage(message);
                }
                finally
                {
                    message.Stream.Position = 0;
                }
            }
            catch (Exception ex)
            {
                if (Logger.HandleException(ex, null))
                    throw;
            }
        }

        protected void ProcessOutgoingMessage(System.Web.Services.Protocols.SoapMessage message)
        {
            try
            {
                
                //copy the contents of the memory stream to the outgoing soap stream
                message.Stream.Position = 0;
                message.Stream.CopyTo(SoapStream);



                //read the stream of the outgoing message
                message.Stream.Position = 0;


                var responseData = Utility.GetResponseFromMessage(message);

                Logger.AddLogItem(RequestData, responseData, ContextType.Server);
                
            }
            catch (Exception ex)
            {
                if (Logger.HandleException(ex, null))
                    throw;
            }
        }

        //allows us to hook into the ingoing/outgoing stream
        public override System.IO.Stream ChainStream(System.IO.Stream stream)
        {
            SoapStream = stream;
            //replace the existing stream with a memory stream that will be filled with the request
            var newStream = new System.IO.MemoryStream();
            return base.ChainStream(newStream);
        }
    }
}
