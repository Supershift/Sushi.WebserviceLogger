﻿using Microsoft.AspNetCore.Http;
using System;

namespace Sushi.WebserviceLogger.Core
{
    public class LoggerOptions<T> where T : LogItem, new()
    {
        /// <summary>
        /// Gets or sets a function that is called just before an instance of <typeparamref name="T"/> is inserted into Elastic.         
        /// This allows the caller to enrich the instance of <typeparamref name="T"/> or even return a new instance of <typeparamref name="T"/>.
        /// If NULL is returned by the function, further processing stops and nothing is inserted into Elastic.
        /// Multiple functions can be chained by using the += operator. Chained functions are called in order of adding. 
        /// If one of the functions returns NULL, execution of the chain stops.
        /// </summary>
        public Func<T, HttpContext, T> AddLogItemCallback { get; set; }

        /// <summary>
        /// Gets or sets a function that is called whenever an <see cref="Exception"/> occurs during logging.
        /// This allows the caller to apply its own logging.
        /// Note: <typeparamref name="T"/> can be NULL.
        /// Return true if the <see cref="Exception"/> needs to be thrown, return false if the <see cref="Exception"/> needs to be surpressed.
        /// </summary>
        public Func<Exception, T, HttpContext, bool> ExceptionCallback { get; set; }

        /// <summary>
        /// Gets or sets a function that allows to set <see cref="LogItem.CorrelationID"/>. 
        /// </summary>
        public Func<HttpContext, string> CorrelationIdCallback { get; set; }

        /// <summary>
        /// Gets or sets the maximum alloweed length of <see cref="Body.Data"/>. Any characters above the limit will be truncated before inserting into ElasticSearch.        
        /// </summary>
        public int MaxBodyContentLength { get; set; } = 4000;

        /// <summary>
        /// Gets or sets a function that will be called to determine indexname. 
        /// The return value will be used as index name. 
        /// The default index name is 'webservicelogs'
        /// </summary>
        public Func<string> IndexNameCallback { get; set; }
    }
}
