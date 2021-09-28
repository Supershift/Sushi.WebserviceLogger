using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Filter
{
    public class MessageLoggerFilterContext
    {   
        public string RequestData { get; set; }
        public string ResponseData { get; set; }
    }
}
