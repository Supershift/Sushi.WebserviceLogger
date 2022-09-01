using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Sushi.WebserviceLogger.Core
{
    /// <summary>
    /// Provides utility methods for the webservice logger.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Returns an instance of <see cref="RequestData"/> based on values found in <paramref name="request"/>.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="started"></param>
        /// <returns></returns>
        public static async Task<RequestData> GetDataFromHttpRequestMessageAsync(System.Net.Http.HttpRequestMessage request, DateTime started)
        {
            var result = new RequestData()
            {
                Url = GetUrlFromUri(request.RequestUri),
                Method = request.Method?.Method,
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
            if(request.Content != null)
            {                
                //get content
                result.Body = new Body()
                {
                    Data = await request.Content.ReadAsStringAsync()
                };
                
                //try to guess content type
                var mediaType = request.Content.Headers?.ContentType?.MediaType;
                if (mediaType != null)
                {
                    result.Body.ContentType = GuessContentTypeFromHeader(mediaType);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns an instance <see cref="ResponseData"/> of based on values found in <paramref name="response"/>.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="started"></param>
        /// <returns></returns>
        public static async Task<ResponseData> GetDataFromHttpResponseMessageAsync(System.Net.Http.HttpResponseMessage response, DateTime started)
        {
            var result = new ResponseData()
            {                
                Started = started,
                 HttpStatusCode = (int)response.StatusCode
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
            if (response.Content != null)
            {
                //get content
                result.Body = new Body()
                {
                    Data = await response.Content.ReadAsStringAsync()
                };                
                
                //try to guess content type
                var mediaType = response.Content.Headers?.ContentType?.MediaType;
                if (mediaType != null)
                {
                    result.Body.ContentType = GuessContentTypeFromHeader(mediaType);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns an instance of <see cref="RequestData"/> based on values found in <paramref name="request"/>.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="started"></param>
        /// <returns></returns>
        public static async Task<RequestData> GetDataFromHttpRequestMessageAsync(HttpRequest request, DateTime started, bool readBody)
        {   
            var result = new RequestData()
            {
                Url = GetUrlFromRequest(request),
                Method = request.Method,
                Started = started
            };

            // get endpoint template
            var endpoint = request.HttpContext?.GetEndpoint() as Microsoft.AspNetCore.Routing.RouteEndpoint;
            if (endpoint != null)
            {
                result.Action = endpoint.RoutePattern?.RawText;
            }

            // get headers from request
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
            // get body
            result.Body = new Body()
            {
                ContentType = GuessContentTypeFromHeader(request.ContentType)
            };

            if (readBody && request.Body != null && request.Body.CanSeek && request.ContentLength > 0 )
            {
                // set stream back to position 0
                request.Body.Position = 0;

                using (var sr = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
                {
                    // get content
                    result.Body.Data = await sr.ReadToEndAsync();                    
                }

                // set stream back to position 0
                request.Body.Position = 0;
            }

            return result;
        }

        /// <summary>
        /// Returns an instance <see cref="ResponseData"/> of based on values found in <paramref name="response"/>.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="started"></param>
        /// <returns></returns>
        public static async Task<ResponseData> GetDataFromHttpResponseMessageAsync(HttpResponse response, DateTime started, bool readBody)
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
            result.Body = new Body()
            {
                ContentType = GuessContentTypeFromHeader(response.ContentType)
            };

            if (readBody && response.Body != null && response.Body.CanRead)
            {
                response.Body.Position = 0;
                using (var sr = new StreamReader(response.Body, Encoding.UTF8, true, 1024, true))
                {
                    //get content
                    result.Body.Data = await sr.ReadToEndAsync();
                }
                //set stream back to position 0
                response.Body.Position = 0;
            }

            return result;
        }

        /// <summary>
        /// Creates an instance of <see cref="Url"/> from an <see cref="HttpRequest"/> object.
        /// </summary>        
        /// <returns></returns>
        public static Url GetUrlFromRequest(HttpRequest request)
        {
            var result = GetUrlFromString(request.GetDisplayUrl());
            return result;
        }

        /// <summary>
        /// Guesses the <see cref="ContentType"/> based on <paramref name="contentTypeHeaderValue"/>.
        /// </summary>
        /// <param name="contentTypeHeaderValue"></param>
        /// <returns></returns>
        public static ContentType GuessContentTypeFromHeader(string contentTypeHeaderValue)
        {
            if (string.IsNullOrWhiteSpace(contentTypeHeaderValue))
                return ContentType.Raw;

            //convert to lowercase            
            contentTypeHeaderValue = contentTypeHeaderValue.ToLower();
            if (contentTypeHeaderValue.Contains("json"))
                return ContentType.JSON;
            if (contentTypeHeaderValue.Contains("xml"))
                return ContentType.XML;
            if (contentTypeHeaderValue.Contains("form"))
                return ContentType.Form;

            return ContentType.Raw;
        }

        /// <summary>
        /// Creates an instance of <see cref="Url"/> from a string representing a URL.
        /// </summary>        
        /// <returns></returns>
        public static Url GetUrlFromString(string url)
        {
            // convert string to uri            
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return GetUrlFromUri(uri);
            else
                return new Url() { AbsoluteUrl = url };
        }

        /// <summary>
        /// Creates an instance of <see cref="Url"/> from a <see cref="Uri"/> object.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Url GetUrlFromUri(Uri uri)
        {
            var result = new Url()
            {
                AbsoluteUrl = uri.AbsoluteUri,
                Address = uri.GetLeftPart(UriPartial.Path),
                Host = uri.Host,
                Path = uri.AbsolutePath,
                Port = uri.Port,
                Scheme = uri.Scheme,
                Query = uri.Query
            };
            
            return result;
        }
    }
}
