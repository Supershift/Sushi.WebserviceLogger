using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;

namespace Sushi.WebserviceLogger.Asmx
{
    internal static class Utility
    {   
        public static RequestData GetRequestFromMessage(SoapMessage message)
        {
            //get context
            var context = System.Web.HttpContext.Current;

            var result = new RequestData()
            {
                Url = Core.Utility.GetUrlFromString(message.Url),
                Method = context?.Request.HttpMethod,
                Started = DateTime.UtcNow,
                ClientIP = context?.Request?.UserHostAddress
            };
            //get headers from request
            if (context?.Request.Headers != null)
            {
                result.Headers = new List<Header>();
                foreach (var headerKey in context.Response.Headers.AllKeys)
                {
                    var headerValue = context.Response.Headers[headerKey];
                    result.Headers.Add(new Header(headerKey, headerValue));
                }
            }
            //get body
            if (message.Stream?.Length > 0)
            {
                //copy contents from message's stream to a memory storage for further processing

                message.Stream.Position = 0;

                //read the contents from the stream to a string
                using (var sr = new StreamReader(message.Stream, Encoding.UTF8, true, 1024, true))
                {
                    result.Body = new Body()
                    {
                        Data = sr.ReadToEnd(),
                        ContentType = ContentType.XML
                    };
                }
            }

            return result;
        }
        
        public static ResponseData GetResponseFromMessage(SoapMessage message)
        {
            //get context
            var context = System.Web.HttpContext.Current;

            //create response
            var result = new ResponseData()
            {
                Started = DateTime.UtcNow,
                HttpStatusCode = context?.Response?.StatusCode
            };

            //get headers from request
            if (context?.Response?.Headers != null)
            {
                result.Headers = new List<Header>();
                foreach (var headerKey in context.Response.Headers.AllKeys)
                {
                    var headerValue = context.Response.Headers[headerKey];
                    result.Headers.Add(new Header(headerKey, headerValue));
                }
            }
            //get body
            if (message.Stream?.Length > 0)
            {
                //copy contents from message's stream to a memory storage for further processing

                message.Stream.Position = 0;

                //read the contents from the stream to a string
                using (var sr = new StreamReader(message.Stream, Encoding.UTF8, true, 1024, true))
                {
                    result.Body = new Body()
                    {
                        Data = sr.ReadToEnd(),
                        ContentType = ContentType.XML
                    };
                }
            }


            //set stream back to position 0
            message.Stream.Position = 0;

            return result;
        }
    }
}
