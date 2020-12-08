using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.WebserviceLogger.Core
{
    /// <summary>
    /// Provides methods to create instances of <see cref="Nest.ElasticClient"/>.
    /// </summary>
    public static class ElasticClientFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="Nest.ElasticClient"/> with provided <see cref="ElasticConfiguration"/>.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static Nest.ElasticClient CreateClient(ElasticConfiguration configuration)
        {
            var settings = new Nest.ConnectionSettings(new Uri(configuration.ElasticUrl))
                    .BasicAuthentication(configuration.ElasticUsername, configuration.ElasticPassword)
                    .ThrowExceptions(true);

            var elasticClient = new Nest.ElasticClient(settings);

            return elasticClient;
        }
    }
}
