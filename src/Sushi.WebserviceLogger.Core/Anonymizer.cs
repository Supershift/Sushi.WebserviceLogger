using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Core
{
    /// <summary>
    /// Contains settings for <see cref="Anonymizer"/>.
    /// </summary>
    public class AnonymizerOptions
    {
        /// <summary>
        /// List of sensitive fields that will have their data removed.
        /// </summary>
        public IEnumerable<string> SensitiveFields { get; set; }
        
        /// <summary>
        /// Sensitive fields' data will be replaced with this value. Defaults to empty string.
        /// </summary>        
        public string Replacement { get; set; } = string.Empty;

        /// <summary>
        /// If set to true, the field matching is case-sensitive.
        /// </summary>
        public bool IsCaseSensitive { get; set; }
    }
    
    public class Anonymizer
    {
        private readonly AnonymizerOptions _options;

        public Anonymizer(IOptions<AnonymizerOptions> options)
        {
            _options = options.Value;
        }
        
        public string AnonymizeJson(string input)
        {
            if(_options?.SensitiveFields == null)
            {
                // nothing to anonymize
                return input;
            }
            
            // todo: do not create new regex everytime
            // build regex 
            var fieldMatcher = string.Join("|", _options.SensitiveFields);
            string pattern = $@"\""({fieldMatcher})\""\s*:\s*(\""[^\""]*\""?|\d+)";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);

            // replace sensitive fields
            var result = regex.Replace(input, $@"""$1"": ""{_options.Replacement}""");

            return result;
        }
    }
}
