﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Sushi.WebserviceLogger.Core;
using System;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Middleware
{
    /// <summary>
    /// Middleware to add webservice logging to the application pipeline.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageLogger<T> where T : LogItem, new()
    {
        /// <summary>
        /// Creates a new instance of <see cref="MessageLogger{T}"/>.
        /// </summary>        
        public MessageLogger(RequestDelegate next, Logger<T> logger)
        {
            _next = next;
            _logger = logger;            
            ContextType = ContextType.Server;
        }

        private readonly RequestDelegate _next;
        private readonly Logger<T> _logger;

        /// <summary>
        /// Gets the current <see cref="ContextType"/>.
        /// </summary>
        public ContextType ContextType { get; protected set; }

        /// <summary>
        /// Invokes the middleware, which first enables bufffering on the request, then calls the next item in pipeline and then logs both the request and response.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            var requestStarted = DateTime.UtcNow;
            var request = context.Request;
            var response = context.Response;

            RequestData requestData = null;
            ResponseData responseData = null;

            //we're going to replace the response's stream in a next step
            var originalResponseStream = response.Body;

            try
            {
                using (var responseBuffer = new System.IO.MemoryStream())
                {
                    try
                    {
                        //load the request's stream into a buffer, to allow multiple reads
                        request.EnableBuffering();
                        //response does not allow this, so we have to create a custom buffer                        
                        response.Body = responseBuffer;
                    }
                    catch (Exception ex)
                    {
                        if (_logger.HandleException(ex, null))
                            throw;
                    }

                    // Call the next delegate/middleware in the pipeline
                    await _next(context);


                    try
                    {
                        //create request and response data objects
                        requestData = await Utility.GetDataFromHttpRequestMessageAsync(request, requestStarted, true);

                        responseData = await Utility.GetDataFromHttpResponseMessageAsync(response, DateTime.UtcNow, true);

                        //copy contents of the buffer to the response (if the mem stream was used)
                        if (responseBuffer.Length > 0)
                        {
                            responseBuffer.Position = 0;
                            await responseBuffer.CopyToAsync(originalResponseStream);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_logger.HandleException(ex, null))
                            throw;
                    }
                }
            }
            finally
            {
                //put the original stream back
                response.Body = originalResponseStream;
            }
            //insert into elastic
            //logger has its own exception handling logic and does not need to be inside try/catch
            if (requestData != null && responseData != null)
                await _logger.AddLogItemAsync(requestData, responseData, ContextType);
        }
    }
}
