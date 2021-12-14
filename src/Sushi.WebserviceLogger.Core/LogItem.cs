using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.WebserviceLogger.Core
{
    /// <summary>
    /// Contains all data associated with one request/response pair.
    /// </summary>    
    public class LogItem
    {
        /// <summary>
        /// Gets or sets a value uniquely identifying an instance of <see cref="LogItem"/>.
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// Gets or sets a value indicating when this <see cref="LogItem"/> was created.
        /// </summary>
        [Nest.Date(Name = "@timestamp")]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating when this <see cref="LogItem"/> was created.
        /// </summary>
        [Obsolete("Use Timestamp instead")]        
        public DateTime Created { get; set; }
        /// <summary>
        /// Gets or sets the moment communication started. 
        /// When <see cref="ContextType"/> is <see cref="ContextType.Client"/>, this indicates when the request was sent.
        /// When <see cref="ContextType"/> is <see cref="ContextType.Server"/>, this indicates when the request was received.
        /// </summary>
        public DateTime? Start { get; set; }
        /// <summary>
        /// Gets or sets the moment communication ended. 
        /// When <see cref="ContextType"/> is <see cref="ContextType.Client"/>, this indicates when the response was received.
        /// When <see cref="ContextType"/> is <see cref="ContextType.Server"/>, this indicates when the response was sent.
        /// </summary>
        public DateTime? End { get; set; }
        /// <summary>
        /// Gets or sets a value in milliseconds indicating the total time passed between request and response.
        /// </summary>
        public int? Duration { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ContextType"/> that this <see cref="LogItem"/> was created. 
        /// </summary>        
        public ContextType ContextType { get; set; }
        
        /// <summary>
        /// Gets or sets the IP address of the client for this webservice call.
        /// </summary>        
        public string ClientIP { get; set; }
        /// <summary>
        /// Gets or sets the UserAgent of the client for this webservice call.
        /// </summary>
        public string ClientUserAgent { get; set; }
        /// <summary>
        /// Gets or sets the IP address of the server for this webservice call.
        /// </summary>         
        public string ServerIP { get; set; }
        /// <summary>
        /// Gets or sets an object containing all request specific data.
        /// </summary>
        public Request Request { get; set; }
        /// <summary>
        /// Gets or sets an object containing all response specific data.
        /// </summary>
        public Response Response { get; set; }

        /// <summary>
        /// Gets or sets a value that allows you to link multiple logitems. 
        /// For instance, all logitems created during the execution of one web request can be linked together by assigning them the same ID.
        /// </summary>           
        public string CorrelationID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating which service was called. By default this is set with <see cref="Url.Address"/>.
        /// </summary>
        public string Service { get; set; }
    }    

    /// <summary>
    /// Represents an http header used in communication between client and server.
    /// </summary>
    public class Header
    {
        /// <summary>
        /// Create an empty instance of <see cref="Header"/>.
        /// </summary>
        public Header()
        {

        }

        /// <summary>
        /// Create an instance of <see cref="Header"/> using supplied <paramref name="key"/> and <paramref name="value"/>.
        /// </summary>
        public Header(string key, string value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Gets or sets the name of the header.
        /// </summary>        
        public string Key { get; set; }
        /// <summary>
        /// Gets or sets the value of the header.
        /// </summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// Contains all request specific data for a webservice call.
    /// </summary>
    public class Request
    {
        /// <summary>
        /// Gets or sets the webservice URL which was called.
        /// </summary>
        public Url Url { get; set; }
        /// <summary>
        /// Gets or sets the HTTP method used to call <see cref="Request.Url"/>, e.g. GET, POST, etc.
        /// </summary>        
        public string Method { get; set; }
        /// <summary>
        /// Gets or sets a collection of httpheaders sent in the request.
        /// </summary>
        public List<Header> Headers { get; set; }
        /// <summary>
        /// Gets or sets an object containing data about the body sent in the request.
        /// </summary>
        public Body Body { get; set; }

        /// <summary>
        /// Gets or sets the soap action if the request is part of a SOAP communication.
        /// </summary>        
        public string SoapAction { get; set; }
    }

    /// <summary>
    /// Contains all response specific data for a webservice call.
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Gets or sets the http status for the response.
        /// </summary>        
        public int? HttpStatusCode { get; set; }
        /// <summary>
        /// Gets or sets a collection of httpheaders sent in the response.
        /// </summary>
        public List<Header> Headers { get; set; }
        /// <summary>
        /// Gets or sets an object containing data about the body sent in the response.
        /// </summary>
        public Body Body { get; set; }
    }

    /// <summary>
    /// Contains all data for the body of a respone or request.
    /// </summary>
    public class Body
    {
        /// <summary>
        /// Gets or sets the <see cref="Core.ContentType"/> of the data in the body.
        /// </summary>        
        public ContentType ContentType { get; set; }
        /// <summary>
        /// Gets or sets the data in the body.
        /// </summary>        
        public string Data { get; set; }

        /// <summary>
        /// Gets ors set a value indicating if the content in <see cref="Body.Data"/> was truncated.
        /// </summary>
        public bool IsDataTruncated { get; set; }
    }

    /// <summary>
    /// Contains properties about an URL.
    /// </summary>
    public class Url
    {
        /// <summary>
        /// Gets or sets the absolute url, e.g. https://www.contoso.com/api/product?name=smith&amp;country=nl.
        /// </summary>
        public string AbsoluteUrl { get; set; }
        /// <summary>
        /// Gets or sets the path part of the url, e.g. /api/product.
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// Gets or sets the address, which is the combination of scheme, authority and path, e.g. https://www.contoso.com/api/product.
        /// </summary>        
        public string Address { get; set; }
        /// <summary>
        /// Gets or sets the host of the url, e.g. www.contoso.com
        /// </summary>        
        public string Host { get; set; }
        /// <summary>
        /// Gets or sets the scheme of the url, e.g. http, https, etc.
        /// </summary>        
        public string Scheme { get; set; }
        /// <summary>
        /// Gets or sets the port of the url.
        /// </summary>
        public int? Port { get; set; }
        /// <summary>
        /// Gets or sets the query part of the url, e.g. ?name=smith&amp;country=nl.
        /// </summary>
        public string Query { get; set; }
    }

    /// <summary>
    /// Defines types of the data in the body of a request or response.
    /// </summary>
    public enum ContentType
    {
        /// <summary>
        /// Plain text or other content types.
        /// </summary>
        Raw,
        /// <summary>
        /// Form data used in requests (for example application/x-www-form-urlencoded)
        /// </summary>
        Form,
        /// <summary>
        /// Data in JSON format (for example application/json)
        /// </summary>
        JSON,
        /// <summary>
        /// Data in XML format (for example application/xml)
        /// </summary>
        XML
    }

    /// <summary>
    /// Defines the context of the application from which the webservice call was performed.
    /// </summary>
    public enum ContextType
    {
        /// <summary>
        /// The application sent a request to a server and received the response.
        /// </summary>
        Client = 1,
        /// <summary>
        /// The application received a request and returned a respone to the client.
        /// </summary>
        Server = 2
    }
}
