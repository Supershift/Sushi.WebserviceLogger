using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;

namespace Sushi.WebserviceLogger.Asmx
{
    /// <summary>
    /// Base class from which to derive a <see cref="SoapExtension"/> to log ASMX Soap traffic.
    /// The derived class cannot be generic.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ServerTracer<T> : SoapExtension where T: LogItem, new()
    {
        /// <summary>
        /// Is called one time if the tracer is applied using web.config.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public override object GetInitializer(Type serviceType)
        {
            return null;
        }

        /// <summary>
        /// Is called one time if the tracer is applied to a specific method with a <see cref="SoapExtensionAttribute"/>.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public override object GetInitializer(LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute)
        {
            return null;
        }

        /// <summary>
        /// Gets or sets the <see cref="Logger{T}"/> used to create logitems. It is advised to set this once in the overridden <see cref="SoapExtension.Initialize(object)"/> method.
        /// </summary>
        public abstract Logger<T> Logger { get; set; }

        private System.IO.Stream SoapStream;

        private RequestData RequestData { get; set; }

        /// <summary>
        /// Is called for each <see cref="SoapMessageStage"/> to retrieve the data needed to create a <see cref="LogItem"/>.
        /// </summary>
        /// <param name="message"></param>
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

        private void ProcessIncomingMessage(SoapMessage message)
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

        private void ProcessOutgoingMessage(System.Web.Services.Protocols.SoapMessage message)
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

        /// <summary>
        /// Buffers the provided soap data in a <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public override Stream ChainStream(Stream stream)
        {
            SoapStream = stream;
            //replace the existing stream with a memory stream that will be filled with the request
            var newStream = new MemoryStream();
            return base.ChainStream(newStream);
        }
    }
}
