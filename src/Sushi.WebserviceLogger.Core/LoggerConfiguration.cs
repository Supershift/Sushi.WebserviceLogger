using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Core
{
    /// <summary>
    /// Represents the configuration used to create a <see cref="Logger{T}"/>.
    /// </summary>
    public class LoggerConfiguration 
    {
        /// <summary>
        /// Creates a new instance of <see cref="LoggerConfiguration"/> using the provided <paramref name="logItemPersister"/>.
        /// </summary>
        /// <param name="logItemPersister"></param>
        public LoggerConfiguration(ILogItemPersister logItemPersister)
        {
            if (logItemPersister == null)
                throw new ArgumentNullException(nameof(logItemPersister));
            
            LogItemPersister = logItemPersister;
        }

        /// <summary>
        /// Gets the instance of <see cref="ILogItemPersister" /> used when a <see cref="Logger{T}"/> uses this configuration.
        /// </summary>
        public ILogItemPersister LogItemPersister { get; }

        /// <summary>
        /// Gets or sets the maximum alloweed length of <see cref="Body.Data"/>. Any characters above the limit will be truncated before inserting into ElasticSearch.        
        /// </summary>
        public int MaxBodyContentLength { get; set; } = 4000;

        
    }
}
