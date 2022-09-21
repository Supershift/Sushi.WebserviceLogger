using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sushi.WebserviceLogger.Test
{
    public class TestBase
    {
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions()
        {
            WriteIndented = true            
        };

        public void WriteResult<T>(T result)
        {
            var type = typeof(T);
            if (result is string || type.IsPrimitive)
            {
                Console.WriteLine(result);
            }
            else
            {
                var line = JsonSerializer.Serialize(result, _options);
                Console.WriteLine(line);
            }
        }
    }
}
