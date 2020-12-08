using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sushi.WebserviceLogger.SampleService
{
    public static class Config
    {
        public static Logger<LogItem> AsmxLogger { get; set; }
    }
}