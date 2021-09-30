using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.SampleService.Filters
{
    public class ErrorFilter : IAsyncExceptionFilter
    {
        public class ErrorResponse
        {
            public int ErrorID { get; set; }
            public string ErrorDescription { get; set; }
        }

        public Task OnExceptionAsync(ExceptionContext context)
        {
            var response = new ErrorResponse()
            {
                ErrorID = 1,
                ErrorDescription = context.Exception.Message
            };
            context.Result = new ObjectResult(response) { StatusCode = 500 };
            
            return Task.CompletedTask;
        }
    }
}
