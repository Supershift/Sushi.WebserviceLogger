using Sushi.WebserviceLogger.Core;
using System;

namespace Sushi.WebserviceLogger.Filter
{
    public class FilterConfigurationBuilder<T> where T : LogItem, new()
    {
        public Action<WebserviceLoggerFilterOptions<T>> FilterOptions;
        public Action<LoggerOptions<T>> LoggerOptions;

        public FilterConfigurationBuilder()
        {

        }
    }
}
