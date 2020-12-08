using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Core.Middleware
{
    /// <summary>
    /// Provides utility methods for the webservice logger in ASP.NET core.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Returns an instance of <see cref="RequestData"/> based on values found in <paramref name="request"/>.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="started"></param>
        /// <returns></returns>
        public static async Task<RequestData> GetDataFromHttpRequestMessageAsync(HttpRequest request, DateTime started)
        {
            var result = new RequestData()
            {
                Url = GetUrlFromRequest(request),
                Method = request.Method,
                Started = started
            };
            //get headers from request
            if (request.Headers != null)
            {
                result.Headers = new List<Header>();
                foreach (var header in request.Headers)
                {
                    foreach (var value in header.Value)
                    {
                        result.Headers.Add(new Header(header.Key, value));
                    }
                }
            }
            //get body
            if (request.Body != null && request.ContentLength > 0)
            {
                //set stream back to position 0
                request.Body.Position = 0;

                using (var sr = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
                {
                    //get content
                    result.Body = new Body()
                    {
                        Data = await sr.ReadToEndAsync()
                    };
                }

                //set stream back to position 0
                request.Body.Position = 0;

                result.Body.ContentType = Core.Utility.GuessContentTypeFromHeader(request.ContentType);                
            }

            return result;
        }

        /// <summary>
        /// Returns an instance <see cref="ResponseData"/> of based on values found in <paramref name="response"/>.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="started"></param>
        /// <returns></returns>
        public static async Task<ResponseData> GetDataFromHttpResponseMessageAsync(HttpResponse response, DateTime started)
        {
            var result = new ResponseData()
            {
                Started = started,
                HttpStatusCode = response.StatusCode
            };
            //get headers from request
            if (response.Headers != null)
            {
                result.Headers = new List<Header>();
                foreach (var header in response.Headers)
                {
                    foreach (var value in header.Value)
                    {
                        result.Headers.Add(new Header(header.Key, value));
                    }
                }
            }
            //get body
            if (response.Body != null && response.Body.CanRead)
            {
                response.Body.Position = 0;
                using (var sr = new StreamReader(response.Body, Encoding.UTF8, true, 1024, true))
                {
                    //get content
                    result.Body = new Body()
                    {
                        Data = await sr.ReadToEndAsync()
                    };
                }
                //set stream back to position 0
                response.Body.Position = 0;

                //try to guess content type
                result.Body.ContentType = Core.Utility.GuessContentTypeFromHeader(response.ContentType); 
            }

            return result;
        }

        /// <summary>
        /// Creates an instance of <see cref="Url"/> from an <see cref="HttpRequest"/> object.
        /// </summary>        
        /// <returns></returns>
        public static Url GetUrlFromRequest(HttpRequest request)
        {
            var result = new Url()
            {
                AbsoluteUrl = request.GetEncodedUrl(),
                Address = request.GetDisplayUrl(),
                Host = request.Host.Value,
                Path = request.Path.Value,
                Port = request.Host.Port,
                Scheme = request.Scheme,
                Query = request.QueryString.Value
            };

            return result;
        }
    }
}
