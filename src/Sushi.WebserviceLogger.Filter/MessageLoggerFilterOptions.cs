using Microsoft.AspNetCore.Http;
using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Filter
{
    /// <summary>
    /// Represents the configuration used to create a <see cref="MessageLoggerFilter"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageLoggerFilterOptions : MessageLoggerFilterOptions<LogItem>
    {

    }

    /// <summary>
    /// Represents the configuration used to create a <see cref="MessageLoggerFilter{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MessageLoggerFilterOptions<T> where T : LogItem, new()
    {
        /// <summary>
        /// Creates a new instance of <see cref="MessageLoggerFilterOptions{T}"/>.
        /// </summary>        
        public MessageLoggerFilterOptions() 
        {
            
        }
        
        /// <summary>
        /// Gets or sets a list of paths that will not be processed by the filter.
        /// </summary>
        public List<string> ExcludePaths { get; set; }

        /// <summary>
        /// Gets or sets a list of paths that will not have their response and request body serialized.
        /// </summary>
        public List<string> ExcludeBodyPaths { get; set; }

        /// <summary>
        /// Called when the filter first executes and receives the request.        
        /// </summary>
        public Action<MessageLoggerFilterContext> OnRequestReceived { get; set; }

        /// <summary>
        /// Called when the action has been executed and the request body has been read by the filter.
        /// </summary>
        public Action<MessageLoggerFilterContext> OnRequestBodyRead { get; set; }

        /// <summary>
        /// Called when the action's result has been written and the response body has been read by the filter.
        /// </summary>
        public Action<MessageLoggerFilterContext> OnResponseBodyRead { get; set; }

        /// <summary>
        /// Called when the filter has parsed all data from request and response and is about to persist the data. 
        /// </summary>
        public Action<MessageLoggerFilterContext> OnLoggingDataCreated { get; set; }

        /// <summary>
        /// Gets or sets the json options used when serializing request and response objects.
        /// </summary>
        public System.Text.Json.JsonSerializerOptions JsonSerializerOptions { get; set; }   
    }
}
