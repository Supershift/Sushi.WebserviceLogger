using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Persisters
{
    public class QueueProcessorOptions
    {
        /// <summary>
        /// Gets or sets the maximum number of items inserted into Elastic in one bulk operation.
        /// </summary>
        public int MaxBatchSize { get; set; } = 100;
        /// <summary>
        /// Gets or sets a delegate which is called when an <see cref="Exception"/> is encountered while performing <see cref="ExecuteAsync(CancellationToken)"/>.
        /// </summary>
        public Action<Exception> ExceptionDelegate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how much time is spent waiting if the persister's buffer is empty.
        /// </summary>
        public TimeSpan WaitTime { get; set; } = TimeSpan.FromSeconds(1);
    }
}
