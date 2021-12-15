using Microsoft.AspNetCore.Http;
using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Filter
{
    public class MessageLoggerFilterContext
    {
        public MessageLoggerFilterContext(HttpContext httpContext)
        {
            HttpContext = httpContext;
        }
        
        /// <summary>
        /// If set to true, no further actions are performed by the <see cref="MessageLoggerFilter{T}"/>.
        /// </summary>
        public bool StopLogging { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="HttpContext"/> for the current request.
        /// </summary>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// Gets or sets the argument passed into the action which is taken from the request's body.
        /// </summary>
        public object RequestObject { get; set; }

        /// <summary>
        /// Gets or sets the object returned by the action as body.
        /// </summary>
        public object ResponseObject { get; set; }

        /// <summary>
        /// Gets or sets an instance of <see cref="RequestData"/> which will be logged.
        /// </summary>
        public RequestData RequestData { get; set; }
        /// <summary>
        /// Gets or sets an instance of <see cref="ResponseData"/> which will be logged.
        /// </summary>
        public ResponseData ResponseData { get; set; }

        /// <summary>
        /// Gets or sets the max length in characters of the response body and request body when logging. All characters above this limit are truncated.
        /// </summary>
        public int MaxBodyContentLength { get; set; }

        /// <summary>
        /// If set to true, the body objects of the request and response will be serialized. True is the default value.
        /// </summary>
        public bool SerializeBody { get; set; } = true;
    }
}
