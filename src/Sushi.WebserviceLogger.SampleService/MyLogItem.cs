using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.SampleService
{
    public class MyLogItem : LogItem
    {
        [Nest.Keyword]
        public string MyKeyword { get; set; }
    }
}
