using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

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
