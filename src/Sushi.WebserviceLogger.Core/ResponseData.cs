using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.WebserviceLogger.Core
{
    /// <summary>
    /// Contains all data specific to one response.
    /// </summary>
    public class ResponseData
    {
        /// <summary>
        /// Gets or sets the moment when the response was first observed by the logger.
        /// </summary>
        public DateTime Started { get; set; }
        /// <summary>
        /// Gets or sets a collection of the response's HTTP headers.
        /// </summary>
        public List<Header> Headers { get; set; }
        /// <summary>
        /// Gets or sets the HTTP status code for the response.
        /// </summary>
        public int? HttpStatusCode { get; set; }
        /// <summary>
        /// Gets or sets an instance of <see cref="Body"/>, containing the content of the request.
        /// </summary>
        public Body Body { get; set; }
    }
}
