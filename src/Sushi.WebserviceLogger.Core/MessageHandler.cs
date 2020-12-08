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
    /// Implementation of <see cref="DelegatingHandler"/> that can be added to a pipeline to generate <see cref="LogItem"/>
    /// Use 'create' factory methods to create instances for specific scenario's.
    /// </summary>    
    public class MessageHandler : MessageHandler<LogItem>
    {
        public MessageHandler(ContextType contextType, Logger<LogItem> logger) : base(contextType, logger)
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="MessageHandler{T}"/> that can be used to log request made with <see cref="HttpClient"/>. 
        /// Requests are logged with an instance of <typeparamref name="T"/>.
        /// An instance of <see cref="HttpClientHandler"/> is used as inner handler.
        /// The instance can be provided as argument when creating an instance of <see cref="HttpClient"/>.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static MessageHandler<T> CreateHttpClientMessageHandler<T>(ElasticConfiguration configuration) where T : LogItem, new()
        {
            var result = new MessageHandler<T>(ContextType.Client, new Logger<T>(configuration));
            result.InnerHandler = new HttpClientHandler();
            return result;
        }

        /// <summary>
        /// Creates an instance of <see cref="MessageHandler{T}"/> that can be used to log requests made to Web API controllers.        
        /// Requests are logged with an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static MessageHandler<T> CreateWebApiMessageHandler<T>(ElasticConfiguration configuration) where T : LogItem, new()
        {
            var result = new MessageHandler<T>(ContextType.Server, new Logger<T>(configuration));
            return result;
        }

        /// <summary>
        /// Creates an instance of <see cref="MessageHandler{T}"/> that can be used to log requests made with <see cref="HttpClient"/>. 
        /// Requests are logged with an instance of <typeparamref name="T"/>.
        /// An instance of <see cref="HttpClientHandler"/> is used as inner handler.
        /// The instance can be provided as argument when creating an instance of <see cref="HttpClient"/>.
        /// </summary>        
        /// <returns></returns>
        public static MessageHandler<T> CreateHttpClientMessageHandler<T>(Logger<T> logger) where T : LogItem, new()
        {
            var result = new MessageHandler<T>(ContextType.Client, logger);
            result.InnerHandler = new HttpClientHandler();
            return result;
        }

        /// <summary>
        /// Creates an instance of <see cref="MessageHandler{T}"/> that can be used to log requests made to Web API controllers.        
        /// Requests are logged with an instance of <typeparamref name="T"/>.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static MessageHandler<T> CreateWebApiMessageHandler<T>(Logger<T> logger) where T : LogItem, new()
        {
            var result = new MessageHandler<T>(ContextType.Server, logger);
            return result;
        }
    }

    /// <summary>
    /// Implementation of <see cref="DelegatingHandler"/> that can be added to a pipeline to generate <typeparamref name="T"/>.
    /// Use <see cref="MessageHandler"/> factory methods to create instances for specific scenario's.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageHandler<T> : DelegatingHandler where T : LogItem, new()
    {
        /// <summary>
        /// Create an instance of <see cref="MessageHandler{T}"/>.
        /// </summary>
        /// <param name="contextType"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public MessageHandler(ContextType contextType, Logger<T> logger)
        {
            ContextType = contextType;            
            Logger = logger;
        }

        /// <summary>
        /// Gets the current <see cref="ContextType"/>.
        /// </summary>
        public ContextType ContextType { get; protected set; }

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

            //load the request's stream into a buffer, to allow multiple reads
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

            //pass through request
            var response = await base.SendAsync(request, cancellationToken);

            RequestData requestData = null;
            ResponseData responseData = null;
            try
            {
                //create request data object
                requestData = await Utility.GetDataFromHttpRequestMessageAsync(request, requestStarted);
                
                //read data from response
                responseData = await Utility.GetDataFromHttpResponseMessageAsync(response, DateTime.UtcNow);
            }
            catch(Exception ex)
            {
                if (Logger.HandleException(ex, null))
                    throw;
            }

            //insert into elastic
            //logger has its own exception handling logic and does not need to be inside try/catch
            if(requestData != null && responseData != null)
                await Logger.AddLogItemAsync(requestData, responseData, ContextType);

            return response;
        }

        
    }
}
