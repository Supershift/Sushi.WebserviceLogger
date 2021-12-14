using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Filter
{
    /// <summary>
    /// Filter to add webservice logging to Web API. The filter is not thread-safe and each request should create a new instance of the filter (ie. do not use 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageLoggerFilter<T> : IActionFilter, IAsyncResourceFilter, IAlwaysRunResultFilter where T : LogItem, new()
    {
        private MessageLoggerFilterOptions<T> _options;
        private readonly Logger<T> _logger;
        private readonly MessageLoggerFilterContext _filterContext;
        
        /// <summary>
        /// Creates a new instance of <see cref="MessageLoggerFilter{T}"/>.
        /// </summary>
        
        public MessageLoggerFilter(MessageLoggerFilterOptions<T> options, Logger<T> logger, IHttpContextAccessor httpContextAccessor)
        {
            _options = options;
            _logger = logger;

            if (_options.CorrelationIdCallback != null)
            {
                _logger.CorrelationIdCallback = () =>
                {
                    return _options.CorrelationIdCallback(httpContextAccessor.HttpContext);
                };
            }
            else
            {
                _logger.CorrelationIdCallback = () => httpContextAccessor.HttpContext.TraceIdentifier;
            }

            //register logitem callback on logger
            if (_options.AddLogItemCallback != null)
            {
                _logger.AddLogItemCallback = (T logItem) =>
                {
                    var callbacks = _options.AddLogItemCallback.GetInvocationList();
                    foreach (var callback in callbacks)
                    {
                        if (logItem != null)
                            logItem = callback.DynamicInvoke(logItem, httpContextAccessor.HttpContext) as T;
                    }

                    return logItem;
                };
            }

            //register exception callback on logger
            if (_options.ExceptionCallback != null)
            {
                _logger.ExceptionCallback = (Exception e, T logItem) =>
                {
                    return _options.ExceptionCallback(e, logItem, httpContextAccessor.HttpContext);
                };
            }

            _logger.IndexNameCallback = _options.IndexNameCallback;
            _logger.MaxBodyContentLength = _options.MaxBodyContentLength;

            _filterContext = new MessageLoggerFilterContext(httpContextAccessor.HttpContext)
            {
                MaxBodyContentLength = _logger.MaxBodyContentLength
            };
        }

        /// <summary>
        /// Called at beginning of pipeline and calls <see cref="MessageLoggerFilterOptions{T}.OnRequestReceived"/>. 
        /// After execution of the pipeline it gathers all data and calls the logger to persist the log data, calling <see cref="MessageLoggerFilterOptions{T}.OnLoggingDataCreated"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            var started = DateTime.UtcNow;
            _options.OnRequestReceived?.Invoke(_filterContext);
            await next();

            if(_filterContext.StopLogging)
            {
                return;
            }

            
            // read all request data
            try
            {   
                _filterContext.RequestData = await Utility.GetDataFromHttpRequestMessageAsync(context.HttpContext.Request, started, false);
                if (_filterContext.RequestObject != null)
                {
                    _filterContext.RequestData.Body.Data = System.Text.Json.JsonSerializer.Serialize(_filterContext.RequestObject, _options.JsonSerializerOptions);
                }
            }
            catch (Exception ex)
            {
                if (_logger.HandleException(ex, null))
                    throw;
            }

            // read all response data
            try
            {
                _filterContext.ResponseData = await Utility.GetDataFromHttpResponseMessageAsync(context.HttpContext.Response, DateTime.UtcNow, false);
                if (_filterContext.ResponseObject != null)
                {
                    _filterContext.ResponseData.Body.Data = System.Text.Json.JsonSerializer.Serialize(_filterContext.ResponseObject, _options.JsonSerializerOptions);
                }
            }
            catch (Exception ex)
            {
                if (_logger.HandleException(ex, null))
                    throw;
            }

            // last event before logger is called
            try
            {
                _options.OnLoggingDataCreated?.Invoke(_filterContext);
            }
            catch (Exception ex)
            {
                if (_logger.HandleException(ex, null))
                    throw;
            }

            if(_filterContext.StopLogging)
            {
                return;
            }

            // this must run outside try/catch, because Logger has its own exception handling logic
            if (_filterContext.RequestData != null && _filterContext.ResponseData != null)
            {
                _logger.MaxBodyContentLength = _filterContext.MaxBodyContentLength;
                await _logger.AddLogItemAsync(_filterContext.RequestData, _filterContext.ResponseData, ContextType.Server);
            }
        }

        /// <summary>
        /// Called before the action is executed, reads the request's body.
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (_filterContext.StopLogging)
            {
                return;
            }
            try
            {
                // get request body data
                // check if there is a parameter on the action that is filled from the body
                var bodyParameter = context.ActionDescriptor.Parameters?.FirstOrDefault(x => x.BindingInfo.BindingSource == Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource.Body);
                if (bodyParameter != null)
                {   
                    var bodyObject = context.ActionArguments[bodyParameter.Name];
                    _filterContext.RequestObject = bodyObject;     
                }
            }
            catch (Exception ex)
            {
                if (_logger.HandleException(ex, null))
                    throw;
            }
        }

        /// <summary>
        /// Called after the action has executed, allows logged request body to be inspected and changed by <see cref="MessageLoggerFilterOptions{T}.OnRequestBodyRead"/> event.
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // the action has been executed and the request object can now be safely passed to clients, because altering it cannot interfere with the action anymore
            _options.OnRequestBodyRead?.Invoke(_filterContext);
        }

        /// <summary>
        /// Called when the result is created but not yet written to the response.
        /// </summary>
        /// <param name="context"></param>
        public void OnResultExecuting(ResultExecutingContext context)
        {
            
        }

        /// <summary>
        /// Called when the result has been written to the response. Reads the result and calls <see cref="MessageLoggerFilterOptions{T}.OnResponseBodyRead"/>.
        /// </summary>
        /// <param name="context"></param>
        public void OnResultExecuted(ResultExecutedContext context)
        {
            if (_filterContext.StopLogging)
            {
                return;
            }
            try
            {
                if (context.Result != null && context.Result is ObjectResult objectResult)
                {
                    _filterContext.ResponseObject = objectResult.Value;
                }
                _options.OnResponseBodyRead?.Invoke(_filterContext);
            }
            catch(Exception ex)
            {
                if (_logger.HandleException(ex, null))
                    throw;
            }
        }
    }
}
