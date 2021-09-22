using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Core
{   
    /// <summary>
    /// Defines an interface to store logitems.
    /// </summary>
    public interface ILogItemPersister 
    {
        /// <summary>
        /// Stores the provided logitem.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logItem"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        Task StoreLogItemAsync<T>(T logItem, string index) where T : LogItem;        
    }
}
