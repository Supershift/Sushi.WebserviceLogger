# Sushi.WebserviceLogger
[![Build Status](https://dev.azure.com/supershift/Mediakiwi/_apis/build/status/Sushi.WebserviceLogger?branchName=main)](https://dev.azure.com/supershift/Mediakiwi/_build/latest?definitionId=100&branchName=main)
[![NuGet version (Sushi.WebserviceLogger.Core)](https://img.shields.io/nuget/v/Sushi.WebserviceLogger.Core.svg?style=flat-square)](https://www.nuget.org/packages/Sushi.WebserviceLogger.Core/)
## Features
Sushi WebserviceLogger is a set of NuGet libraries that allows you to easily log traffic from and to webservices into Elasticsearch.
## Requirements
* ElasticSearch 7.7 or higher
* .Net Standard 2.0
## Quick start
Log outgoing traffic from an HttpClient:
```csharp
var elasticConfig = new Core.ElasticConfiguration("elastic url", "elastic user", "elastic password");

var loggingHandler = Core.MessageHandler.CreateHttpClientMessageHandler<LogItem>(elasticConfig);

// make sure your Client is only initialized once per application
var client = new System.Net.Http.HttpClient(loggingHandler);     

// call an endpoint
using (var response = await Client.GetAsync("https://www.github.com"))
{
    Console.WriteLine(response.StatusCode);
}

// your outgoing call and the response are now logged in Elastic
```

## Additional resources
See the individual documentation for each package for more detailed instructions.
* Sushi.WebserviceLogger.Core.Middleware
  * Provides middleware to log incoming calls to ASP.NET Core
* Sushi.WebserviceLogger.Asmx
  * Provides a SoapExtension to log incomming SOAP calls to ASP.NET ASMX webservices
