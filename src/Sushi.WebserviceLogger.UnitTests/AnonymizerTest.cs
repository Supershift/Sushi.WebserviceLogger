using Microsoft.Extensions.Options;
using Sushi.WebserviceLogger.Core;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Sushi.WebserviceLogger.UnitTests
{
    public class AnonymizerTest
    {
        private readonly ITestOutputHelper _output;

        public AnonymizerTest(ITestOutputHelper output)
        {
            _output = output;
        }
        
        [Theory]
        [InlineData("password", "{\"field\":\"someValue\",\"password\":\"my!Passw0rd\",\"anotherField\":5}", "my!Passw0rd")]
        [InlineData("password", "{\"field\":\"someValue\",\"password\":\"my!Passw", "my!Passw")]
        [InlineData("Password", "{\"field\":\"someValue\",\"password\":\"my!Passw0rd\",\"anotherField\":5}", "my!Passw0rd")]
        [InlineData("password", "{\"field\":\"someValue\",\"Password\":\"my!Passw0rd\",\"anotherField\":5}", "my!Passw0rd")]
        [InlineData("id", "{\"field\":\"field\",\"id\":1234,\"anotherField\":5}", "1234")]
        public void AnonymizeJson(string field, string data, string sensitiveData)
        {
            var options = Options.Create(new AnonymizerOptions() { IsCaseSensitive = true, SensitiveFields = new[] { field } });
            var anonymizer = new Anonymizer(options);

            // check if the sensitive field is in the input
            Assert.Contains(sensitiveData, data);

            var result = anonymizer.AnonymizeJson(data);

            _output.WriteLine(result);
            // check if the sensitive field is no longer in the input
            Assert.DoesNotContain(sensitiveData, result);
        }
    }
}