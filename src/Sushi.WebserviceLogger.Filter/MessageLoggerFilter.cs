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
    public class MessageLoggerFilter<T> : IAsyncActionFilter, IAsyncResourceFilter, IAsyncAlwaysRunResultFilter, IAsyncExceptionFilter where T : LogItem, new()
    {
        protected MessageLoggerFilterConfiguration<T> Config { get; }
        protected Logger<T> Logger { get; }
        protected MessageLoggerFilterContext FilterContext {get;}

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

            FilterContext = new MessageLoggerFilterContext();            
        }

        // triggers at beginning of pipeline 
        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            var started = DateTime.UtcNow;
            await next();

            // everything has been executed, create the log item            
            var requestData = await Utility.GetDataFromHttpRequestMessageAsync(context.HttpContext.Request, started, false);
            requestData.Body.Data = FilterContext.RequestData;
            
            var responseData = await Utility.GetDataFromHttpResponseMessageAsync(context.HttpContext.Response, DateTime.UtcNow, false);
            responseData.Body.Data = FilterContext.ResponseData;

            await Logger.AddLogItemAsync(requestData, responseData, ContextType.Server);
        }

        // triggers right before the controller's action is executed
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // first execute the action
            await next();

            // get request body data
            // check if there is a parameter on the action that is filled from the body
            var bodyParameter = context.ActionDescriptor.Parameters?.FirstOrDefault(x => x.BindingInfo.BindingSource == Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource.Body);
            if (bodyParameter != null)
            {
                var bodyObject = context.ActionArguments[bodyParameter.Name];

                // serialize the body object
                FilterContext.RequestData = System.Text.Json.JsonSerializer.Serialize(bodyObject);                 
            }
        }

        // triggers when the controller returns a result    
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            // first let all other result filters run, as they may change the result
            await next();

            // add response body data
            FilterContext.ResponseData = GetBodyFromResult(context.Result);            
        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            FilterContext.ResponseData = GetBodyFromResult(context.Result);
        }

        protected string GetBodyFromResult(IActionResult actionResult)
        {
            string result = null;
            if (actionResult != null && actionResult is ObjectResult objectResult)
            {
                // serialize the result body object
                result = System.Text.Json.JsonSerializer.Serialize(objectResult.Value);
            }
            return result;
        }
    }
}
