using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.WebserviceLogger.Core
{
    /// <summary>
    /// Contains all settings to configure the webservice logger.
    /// </summary>
    public class ElasticConfiguration 
    {
        /// <summary>
        /// Creates an instance of <see cref="ElasticConfiguration"/> with the minimum required config settings. Properties of the instance can be used to set other options.
        /// </summary>
        /// <param name="elasticUrl"></param>
        /// <param name="elasticUsername"></param>
        /// <param name="elasticPassword"></param>
        public ElasticConfiguration(string elasticUrl, string elasticUsername, string elasticPassword)
        {
            if (string.IsNullOrWhiteSpace(elasticUrl))
                throw new ArgumentNullException(nameof(elasticUrl));
            if (string.IsNullOrWhiteSpace(elasticUsername))
                throw new ArgumentNullException(nameof(elasticUsername));
            if (string.IsNullOrWhiteSpace(elasticPassword))
                throw new ArgumentNullException(nameof(elasticPassword));

            ElasticUrl = elasticUrl;
            ElasticUsername = elasticUsername;
            ElasticPassword = elasticPassword;
        }

        /// <summary>
        /// Gets the URL for the Elastic instance the webservice logger will use.
        /// </summary>
        public string ElasticUrl { get; private set; }
        /// <summary>
        /// Gets the username for the Elastic instance the webservice logger will use.
        /// </summary>
        public string ElasticUsername { get; private set; }
        /// <summary>
        /// Gets the password for the Elastic instance the webservice logger will use.
        /// </summary>
        public string ElasticPassword { get; private set; }

        
    }    
}
