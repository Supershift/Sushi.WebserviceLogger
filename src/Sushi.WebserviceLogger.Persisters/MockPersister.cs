using Microsoft.Extensions.Options;
using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Persisters
{
    public class MockPersisterOptions
    {
        /// <summary>
        /// Is invoked when a log item is stored on the persister.
        /// </summary>
        public Action<LogItem, string> Callback;
    }
    
    /// <summary>
    /// Mock persister which does not store the <see cref="LogItem"/> to anything. You can specify a delegate which is called when a persister would write to Elastic.
    /// </summary>
    public class MockPersister : ILogItemPersister
    {
        private readonly MockPersisterOptions _options;

        public MockPersister(IOptions<MockPersisterOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// If specified, invokes <see cref="Callback"/> with the specified parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logItem"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public Task StoreLogItemAsync<T>(T logItem, string index) where T : LogItem
        {
            if (_options.Callback != null)
                _options.Callback(logItem, index);
            
            return Task.CompletedTask;
        }
    }
}
