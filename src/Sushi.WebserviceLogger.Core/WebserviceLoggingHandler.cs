using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Core
{
    /// <summary>
    /// Implementation of <see cref="DelegatingHandler"/> that can be added to a pipeline to generate <typeparamref name="T"/>.
    /// Use <see cref="MessageHandler"/> factory methods to create instances for specific scenario's.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WebserviceLoggingHandler<T> : DelegatingHandler where T : LogItem, new()
    {
        /// <summary>
        /// Create an instance of <see cref="WebserviceLoggingHandler{T}"/>.
        /// </summary>
        /// <param name="contextType"></param>        
        /// <param name="logger"></param>
        public WebserviceLoggingHandler(Logger<T> logger)
        {
            Logger = logger;
        }

        /// <summary>
        /// Gets the instance of <see cref="Logger{T}"/> used to log. 
        /// Callback functions can be set on this instance.
        /// </summary>
        public Logger<T> Logger { get; protected set; }

        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation. 
        /// The HTTP request and resulting response are logged using the MessageHandler's <see cref="Logger{T}"/>.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {   
            var requestStarted = DateTime.UtcNow;

            // load the request's stream into a buffer, to allow multiple reads
            try
            {
                if (request?.Content != null)
                    await request.Content.LoadIntoBufferAsync();
            }
            catch(Exception ex)
            {
                if (Logger.HandleException(ex, null))
                    throw;
            }

            // pass through request
            var response = await base.SendAsync(request, cancellationToken);

            RequestData requestData = null;
            ResponseData responseData = null;
            try
            {
                // create request data object
                requestData = await Utility.GetDataFromHttpRequestMessageAsync(request, requestStarted);
                
                // read data from response
                responseData = await Utility.GetDataFromHttpResponseMessageAsync(response, DateTime.UtcNow);
            }
            catch(Exception ex)
            {
                if (Logger.HandleException(ex, null))
                    throw;
            }

            // insert into elastic
            // logger has its own exception handling logic and does not need to be inside try/catch
            if(requestData != null && responseData != null)
                await Logger.AddLogItemAsync(requestData, responseData, ContextType.Client);

            return response;
        }

        
    }
}
