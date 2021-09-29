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
    public class MessageLoggerFilter<T> : IActionFilter, IAsyncResourceFilter, IAlwaysRunResultFilter where T : LogItem, new()
    {
        protected MessageLoggerFilterConfiguration<T> Config { get; }
        protected Logger<T> Logger { get; }
        protected MessageLoggerFilterContext FilterContext { get; }

        public MessageLoggerFilter(MessageLoggerFilterConfiguration<T> config, IHttpContextAccessor httpContextAccessor)
        {
            Config = config;

            // create a logger, and hook up the events defined in config
            Logger = new Logger<T>(Config.Persister);

            if (Config.CorrelationIdCallback != null)
            {
                Logger.CorrelationIdCallback = () =>
                {
                    return Config.CorrelationIdCallback(httpContextAccessor.HttpContext);
                };
            }
            else
            {
                Logger.CorrelationIdCallback = () => httpContextAccessor.HttpContext.TraceIdentifier;
            }

            //register logitem callback on logger
            if (Config.AddLogItemCallback != null)
            {
                Logger.AddLogItemCallback = (T logItem) =>
                {
                    var callbacks = Config.AddLogItemCallback.GetInvocationList();
                    foreach (var callback in callbacks)
                    {
                        if (logItem != null)
                            logItem = callback.DynamicInvoke(logItem, httpContextAccessor.HttpContext) as T;
                    }

                    return logItem;
                };
            }

            //register exception callback on logger
            if (Config.ExceptionCallback != null)
            {
                Logger.ExceptionCallback = (Exception e, T logItem) =>
                {
                    return Config.ExceptionCallback(e, logItem, httpContextAccessor.HttpContext);
                };
            }

            Logger.IndexNameCallback = Config.IndexNameCallback;
            Logger.MaxBodyContentLength = Config.MaxBodyContentLength;

            FilterContext = new MessageLoggerFilterContext(httpContextAccessor.HttpContext)
            {
                
            };
        }

        // triggers at beginning of pipeline 
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            var started = DateTime.UtcNow;
            Config.OnRequestReceived?.Invoke(FilterContext);
            await next();

            if(FilterContext.StopLogging)
            {
                return;
            }

            
            // read all request data
            try
            {   
                FilterContext.RequestData = await Utility.GetDataFromHttpRequestMessageAsync(context.HttpContext.Request, started, false);
                if (FilterContext.RequestObject != null)
                {
                    FilterContext.RequestData.Body.Data = System.Text.Json.JsonSerializer.Serialize(FilterContext.RequestObject);
                }
            }
            catch (Exception ex)
            {
                if (Logger.HandleException(ex, null))
                    throw;
            }

            // read all response data
            try
            {
                FilterContext.ResponseData = await Utility.GetDataFromHttpResponseMessageAsync(context.HttpContext.Response, DateTime.UtcNow, false);
                if (FilterContext.ResponseObject != null)
                {
                    FilterContext.ResponseData.Body.Data = System.Text.Json.JsonSerializer.Serialize(FilterContext.ResponseObject);
                }
            }
            catch (Exception ex)
            {
                if (Logger.HandleException(ex, null))
                    throw;
            }

            // last event before logger is called
            try
            {
                Config.OnLoggingDataCreated?.Invoke(FilterContext);
            }
            catch (Exception ex)
            {
                if (Logger.HandleException(ex, null))
                    throw;
            }

            if(FilterContext.StopLogging)
            {
                return;
            }

            // this must run outside try/catch, because Logger has its own exception handling logic
            if (FilterContext.RequestData != null && FilterContext.ResponseData != null)
            {
                await Logger.AddLogItemAsync(FilterContext.RequestData, FilterContext.ResponseData, ContextType.Server);
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (FilterContext.StopLogging)
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
                    FilterContext.RequestObject = bodyObject;     
                }
            }
            catch (Exception ex)
            {
                if (Logger.HandleException(ex, null))
                    throw;
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // the action has been executed and the request object can now be safely passed to clients, because altering it cannot interfere with the action anymore
            Config.OnRequestBodyRead?.Invoke(FilterContext);
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            
        }

        // triggers when there is a result, and the result cannot be changed anymore because the response has already started
        public void OnResultExecuted(ResultExecutedContext context)
        {
            if (FilterContext.StopLogging)
            {
                return;
            }
            try
            {
                if (context.Result != null && context.Result is ObjectResult objectResult)
                {
                    FilterContext.ResponseObject = objectResult.Value;
                }
                Config.OnResponseBodyRead?.Invoke(FilterContext);
            }
            catch(Exception ex)
            {
                if (Logger.HandleException(ex, null))
                    throw;
            }
        }
    }
}
