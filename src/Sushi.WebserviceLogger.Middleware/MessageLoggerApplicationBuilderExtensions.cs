using Microsoft.AspNetCore.Builder;
using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.WebserviceLogger.Middleware
{
    /// <summary>
    /// Extension methods for <see cref="IApplicationBuilder"/> to add <see cref="MessageLogger{T}"/> to the request execution pipeline.
    /// </summary>
    public static class MessageLoggerApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds <see cref="MessageLogger{T}"/> to the <see cref="IApplicationBuilder"/> request execution pipeline.
        /// </summary>
        public static IApplicationBuilder UseMessageLogger<T>(this IApplicationBuilder builder, MessageLoggerConfig<T> config) where T : LogItem, new()
        {
            return builder.UseMiddleware<MessageLogger<T>>(config);
        }


    }
}
