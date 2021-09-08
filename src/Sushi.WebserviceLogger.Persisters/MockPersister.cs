using Sushi.WebserviceLogger.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Persisters
{
    /// <summary>
    /// Mock persister which does not store the <see cref="LogItem"/> to anything. You can specify a delegate which is called when a persister would write to Elastic.
    /// </summary>
    public class MockPersister : ILogItemPersister
    {
        /// <summary>
        /// Is invoked when <see cref="StoreLogItem{T}(T, string)"/> or <see cref="StoreLogItemAsync{T}(T, string)"/> is called.
        /// </summary>
        public Action<LogItem, string> Callback;
        
        /// <summary>
        /// If specified, invokes <see cref="Callback"/> with the specified parameters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logItem"></param>
        /// <param name="index"></param>
        public void StoreLogItem<T>(T logItem, string index) where T : LogItem
        {
            if (Callback != null)
                Callback(logItem, index);
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
            if (Callback != null)
                Callback(logItem, index);
            
            return Task.CompletedTask;
        }
    }
}
