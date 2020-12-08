using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Core
{
    /// <summary>
    /// Sends request/response data to ElasticSearch using <see cref="LogItem"/>.
    /// </summary>
    public class Logger : Logger<LogItem>
    {
        /// <summary>
        /// Creates an instance of <see cref="Logger"/>.
        /// </summary>
        /// <param name="configuration"></param>
        public Logger(ElasticConfiguration configuration) : base(configuration)
        {

        }

        /// <summary>
        /// Creates an instance of <see cref="Logger{T}"/>.
        /// </summary>        
        public Logger(ILogItemPersister persister) : base(persister)
        {
            
        }
    }
         
    
    /// <summary>
    /// Sends request/response data to ElasticSearch using <typeparamref name="T"/>.
    /// </summary>
    public class Logger<T> where T : LogItem, new()
    {
        ILogItemPersister LogItemPersister;
        /// <summary>
        /// Creates an instance of <see cref="Logger{T}"/>.
        /// </summary>        
        public Logger(ILogItemPersister persister)
        {
            LogItemPersister = persister;
        }


        /// <summary>
        /// Creates an instance of <see cref="Logger{T}"/> using <see cref="InProcessPersister"/>.
        /// </summary>
        /// <param name="configuration"></param>
        public Logger(ElasticConfiguration configuration) : this(new InProcessPersister(configuration)) { }                

        /// <summary>
        /// Gets or sets a function that is called just before an instance of <typeparamref name="T"/> is inserted into Elastic.         
        /// This allows the caller to enrich the instance of <typeparamref name="T"/> or even return a new instance of <typeparamref name="T"/>.
        /// If NULL is returned by the function, further processing stops and nothing is inserted into Elastic.
        /// Multiple functions can be chained by using the += operator. Chained functions are called in order of adding. 
        /// If one of the functions returns NULL, execution of the chain stops.
        /// </summary>
        public Func<T,T> AddLogItemCallback { get; set; }
        
        /// <summary>
        /// Gets or sets a function that is called whenever an <see cref="Exception"/> occurs during logging.
        /// This allows the caller to apply its own logging.
        /// Note: <typeparamref name="T"/> can be NULL.
        /// Return true if the <see cref="Exception"/> needs to be thrown, return false if the <see cref="Exception"/> needs to be surpressed.
        /// </summary>
        public Func<Exception, T, bool> ExceptionCallback { get; set; }

        /// <summary>
        /// Gets or sets a function that allows to set <see cref="LogItem.CorrelationID"/>. 
        /// </summary>
        public Func<string> CorrelationIdCallback { get; set; }

        /// <summary>
        /// Gets or sets the maximum alloweed length of <see cref="Body.Data"/>. Any characters above the limit will be truncated before inserting into ElasticSearch.        
        /// </summary>
        public int MaxBodyContentLength { get; set; } = 4000;

        /// <summary>
        /// Gets or sets a function that will be called to determine indexname. The datetime of the log item will be sent as parameter to the function. 
        /// The return value will be used as index name. 
        /// The default index name is 'webservicelogsYYYYmm', e.g. webservicelogs201910
        /// </summary>
        public Func<DateTime, string> IndexNameCallback { get; set; }

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
                if (IndexNameCallback != null)
                    index = IndexNameCallback(logItem.Start.Value);
                if (string.IsNullOrWhiteSpace(index))
                    index = "webservicelogs" + logItem.Start.Value.ToString("yyyyMM");

                //use persister to index item
                await LogItemPersister.StoreLogItemAsync(logItem, index);                

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
        /// Add a single log item with request/response data into ElasticSearch.
        /// </summary>
        /// <returns></returns>
        public T AddLogItem(RequestData requestData, ResponseData responseData, ContextType contextType)
        {
            T logItem = null;
            try
            {
                //validate configuration

                //create log item
                logItem = CreateLogItem(requestData, responseData, contextType);

                //set index name
                string index = null;
                if (IndexNameCallback != null)
                    index = IndexNameCallback(logItem.Start.Value);
                if (string.IsNullOrWhiteSpace(index))
                    index = "webservicelogs" + logItem.Start.Value.ToString("yyyyMM");

                //use persister to index item
                LogItemPersister.StoreLogItem(logItem, index);

                return logItem;
            }
            catch (Exception ex)
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
                Id = Guid.NewGuid().ToString(),
                Request = new Request()
                {
                    Url = requestData.Url,
                    Method = requestData.Method,
                    Body = requestData.Body,
                    Headers = requestData.Headers,
                    SoapAction =requestData.SoapAction
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

            //set duration if request and response dates are known
            if (logItem.Start != null && logItem.End != null)
                logItem.Duration = (int)(logItem.End - logItem.Start).GetValueOrDefault().TotalMilliseconds;

            //set index name
            string index = null;
            if (IndexNameCallback != null)
                index = IndexNameCallback(logItem.Start.Value);
            if (string.IsNullOrWhiteSpace(index))
                index = "webservicelogs" + logItem.Start.Value.ToString("yyyyMM");

            //call delegate correlationID  if defined
            if (CorrelationIdCallback != null)
                logItem.CorrelationID = CorrelationIdCallback();

            //call delegate logitem function if defined
            if (AddLogItemCallback != null)
            {
                var callbacks = AddLogItemCallback.GetInvocationList();
                foreach (var callback in callbacks)
                {
                    if (logItem != null)
                        logItem = callback.DynamicInvoke(logItem) as T;
                }
            }
            //logItem = AddLogItemCallback(logItem);

            if (logItem == null)
                return null;

            //truncate log item body fields if too long
            if (requestData?.Body?.Data?.Length > MaxBodyContentLength)
            {
                requestData.Body.Data = requestData.Body.Data.Substring(0, MaxBodyContentLength);
                requestData.Body.IsDataTruncated = true;
            }
            if (responseData?.Body?.Data?.Length > MaxBodyContentLength)
            {
                responseData.Body.Data = responseData.Body.Data.Substring(0, MaxBodyContentLength);
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
            if (ExceptionCallback != null)
            {
                throwException = ExceptionCallback(ex, logItem);
            }
            return throwException;
        }
    }
}
