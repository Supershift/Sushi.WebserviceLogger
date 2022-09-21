using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Core
{   
    /// <summary>
    /// Sends request/response data to ElasticSearch using <typeparamref name="T"/>.
    /// </summary>
    public class Logger<T> where T : LogItem, new()
    {
        private readonly ILogItemPersister _persister;
        private readonly LoggerOptions<T> _options;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Creates an instance of <see cref="Logger{T}"/>.
        /// </summary>
        /// <param name="persister"></param>        
        public Logger(ILogItemPersister persister, IOptionsMonitor<LoggerOptions<T>> options, IHttpContextAccessor httpContextAccessor = null)
        {
            _persister = persister;
            _options = options.Get(string.Empty);
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Creates an instance of <see cref="Logger{T}"/>.
        /// </summary>
        /// <param name="persister"></param>
        public Logger(ILogItemPersister persister, LoggerOptions<T> options, IHttpContextAccessor httpContextAccessor = null)
        {
            _persister = persister;
            _options = options;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Add a single log item with request/response data into ElasticSearch.
        /// </summary>
        /// <returns></returns>
        public async Task<T> AddLogItemAsync(RequestData requestData, ResponseData responseData, ContextType contextType)
        {
            T logItem = null;
            try
            {
                //validate configuration

                //create log item
                logItem = CreateLogItem(requestData, responseData, contextType);

                //set index name
                string index = null;
                if (_options.IndexNameCallback != null)
                    index = _options.IndexNameCallback();
                if (string.IsNullOrWhiteSpace(index))
                    index = "webservicelogs";

                //use persister to index item
                await _persister.StoreLogItemAsync(logItem, index);                

                return logItem;
            }
            catch(Exception ex)
            {
                if (HandleException(ex, logItem))
                    throw;
                return logItem;
            }
        }

        

        /// <summary>
        /// Creates a log item with the provided data.
        /// </summary>
        /// <returns></returns>
        private T CreateLogItem(RequestData requestData, ResponseData responseData, ContextType contextType)
        {
            T logItem = null;

            //validate input
            if (requestData == null)
                throw new ArgumentNullException(nameof(requestData));
            if (responseData == null)
                throw new ArgumentNullException(nameof(responseData));

            //create log item
            logItem = new T()
            {
                ClientIP = requestData.ClientIP,
                ContextType = contextType,
                Created = DateTime.UtcNow,
                Timestamp = DateTime.UtcNow,
                Id = Guid.NewGuid().ToString(),
                Request = new Request()
                {
                    Url = requestData.Url,
                    Method = requestData.Method,
                    Body = requestData.Body,
                    Headers = requestData.Headers,                    
                    Action = requestData.Action,
                },
                Response = new Response()
                {
                    Body = responseData.Body,
                    Headers = responseData.Headers,
                    HttpStatusCode = responseData.HttpStatusCode
                },
                Start = requestData.Started,
                End = responseData.Started,
                Service = requestData.Url?.Address
            };

            // set service from template, path or address as fallback
            if (!string.IsNullOrWhiteSpace(requestData.Action))
                logItem.Service = requestData.Action;
            else if (!string.IsNullOrWhiteSpace(requestData.Url?.Path))
                logItem.Service = requestData.Url?.Path;
            else
                logItem.Service = requestData.Url?.Address;

            //set duration if request and response dates are known
            if (logItem.Start != null && logItem.End != null)
                logItem.Duration = (int)(logItem.End - logItem.Start).GetValueOrDefault().TotalMilliseconds;

            //set index name
            string index = null;
            if (_options.IndexNameCallback != null)
                index = _options.IndexNameCallback();
            if (string.IsNullOrWhiteSpace(index))
                index = "webservicelogs" + logItem.Start.Value.ToString("yyyyMM");

            //call delegate correlationID  if defined
            if (_options.CorrelationIdCallback != null)
                logItem.CorrelationID = _options.CorrelationIdCallback(_httpContextAccessor?.HttpContext);

            //call delegate logitem function if defined
            if (_options.AddLogItemCallback != null)
            {
                var callbacks = _options.AddLogItemCallback.GetInvocationList();
                foreach (var callback in callbacks)
                {
                    if (logItem != null)
                        logItem = callback.DynamicInvoke(logItem, _httpContextAccessor?.HttpContext) as T;
                }
            }
            //logItem = AddLogItemCallback(logItem);

            if (logItem == null)
                return null;

            //truncate log item body fields if too long
            if (requestData?.Body?.Data?.Length > _options.MaxBodyContentLength)
            {
                requestData.Body.Data = requestData.Body.Data.Substring(0, _options.MaxBodyContentLength);
                requestData.Body.IsDataTruncated = true;
            }
            if (responseData?.Body?.Data?.Length > _options.MaxBodyContentLength)
            {
                responseData.Body.Data = responseData.Body.Data.Substring(0, _options.MaxBodyContentLength);
                responseData.Body.IsDataTruncated = true;
            }

            return logItem;
        }

        /// <summary>
        /// Calls <see cref="ExceptionCallback"/> with <paramref name="ex"/>. Returns true if <paramref name="ex"/> needs to be thrown by caller.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="logItem"></param>
        /// <returns></returns>
        public bool HandleException(Exception ex, T logItem)
        {
            //call the exception logging handler
            //if it returns true, throw the exception
            //if there is no logging handler, always throw the exception
            bool throwException = true;
            if (_options.ExceptionCallback != null)
            {
                throwException = _options.ExceptionCallback(ex, logItem, _httpContextAccessor?.HttpContext);
            }
            return throwException;
        }
    }
}
