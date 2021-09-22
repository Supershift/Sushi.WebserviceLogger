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
    public class MessageLoggerFilter<T> : IAsyncActionFilter, IAsyncResourceFilter, IAsyncAlwaysRunResultFilter, IAsyncExceptionFilter where T : LogItem
    {
        private Guid _guid = Guid.NewGuid();
        private MessageLoggerFilterConfiguration<T> _config;

        public MessageLoggerFilter(MessageLoggerFilterConfiguration<T> config)
        {
            _config = config;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {   
            await next();

            // only trigger if path contains 'filter'
            if (context.HttpContext.Request.Path.StartsWithSegments("/filter"))
            {
                // check if there is a paramter that is taken from the body
                var bodyParameter = context.ActionDescriptor.Parameters?.FirstOrDefault(x => x.BindingInfo.BindingSource == Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource.Body);
                if (bodyParameter != null)
                {
                    var bodyObject = context.ActionArguments[bodyParameter.Name];

                    // serialize the body object
                    var data = System.Text.Json.JsonSerializer.Serialize(bodyObject);

                }
            }
        }

        public async Task OnExceptionAsync(ExceptionContext context)
        {
            
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
        {
            int i = 0;
            await next();
            i++;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            // only trigger if path contains 'filter'
            if (context.HttpContext.Request.Path.StartsWithSegments("/filter") && context.Result != null)
            {
                if (context.Result is ObjectResult objectResult)
                {

                    // serialize the result body object
                    var data = System.Text.Json.JsonSerializer.Serialize(objectResult.Value);
                }
                
            }

            await next();
            
        }
    }
}
