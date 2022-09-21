using Nest;

namespace Sushi.Elastic.ClientFactory
{
    /// <summary>
    /// Provides options to configure the creation of an <see cref="IElasticClient"/>.
    /// </summary>
    public class ElasticClientFactoryOptions
    {
        /// <summary>
        /// Method called to create a new instance of <see cref="IElasticClient"/>.
        /// </summary>
        public Func<IElasticClient> CreateElasticClient { get; set; }
    }
}
