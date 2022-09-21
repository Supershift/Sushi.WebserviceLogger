using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Filter
{
    public class FilterLogger<T> : Logger<T> where T : LogItem, new()
    {   
        public FilterLogger(ILogItemPersister persister, IOptionsMonitor<LoggerOptions<T>> options, IHttpContextAccessor httpContextAccessor = null) 
            : base (persister, options.Get(typeof(FilterLogger<T>).FullName), httpContextAccessor)
        {

        }
    }
}
