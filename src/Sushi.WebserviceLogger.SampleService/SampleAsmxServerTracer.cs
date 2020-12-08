using Sushi.WebserviceLogger.Asmx;
using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sushi.WebserviceLogger.SampleService
{
    public class SampleAsmxServerTracer : ServerTracer<LogItem>
    {
        public override Logger<LogItem> Logger { get; set; }

        public override void Initialize(object initializer)
        {
            Logger = Config.AsmxLogger;
        }
    }
}