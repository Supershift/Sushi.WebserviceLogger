using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.WebserviceLogger.Core
{
    /// <summary>
    /// Contains all data specific to one request.
    /// </summary>
    public class RequestData
    {
        /// <summary>
        /// Gets or sets an instance of <see cref="Url"/>.
        /// </summary>
        public Url Url { get; set; }
        /// <summary>
        /// Gets or sets the HTTP method used to make the request, ie. POST, GET, etc.
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// Gets or sets the moment when the request was first observed by the logger.
        /// </summary>
        public DateTime Started { get; set; }
        /// <summary>
        /// Gets or sets a collection of the request's HTTP headers.
        /// </summary>
        public List<Header> Headers { get; set; }
        /// <summary>
        /// Gets or sets the IP address of the client sending the request.
        /// </summary>
        public string ClientIP { get; set; }        
        /// <summary>
        /// Gets or sets an instance of <see cref="Body"/>, containing the content of the request.
        /// </summary>
        public Body Body { get; set; }

        /// <summary>
        /// If it can be deduced, this contains the REST endpoint's template, ie. api/object/{id} instead of api/object/165745
        /// </summary>
        public string Action { get; set; }
    }
}
