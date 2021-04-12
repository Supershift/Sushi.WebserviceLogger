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
## Error handling
You can specify a delegate which will be called if an exception occurs during logging. This allows you to log the exception and take further action. The delegate needs to return a boolean, indicating if the exception needs to be thrown (true) or if the exception was handled (false).
```csharp
public bool ExceptionCallback(Exception exception, LogItem logItem)
{
    // log the exception
    Console.WriteLine(exception);
	
    // do not throw the exception
    return false;
}
```
Add the callback to your logger instance:
```csharp
logger.ExceptionCallback += ExceptionCallback;
```
## Additional resources
See the individual documentation for each package for more detailed instructions.
* Sushi.WebserviceLogger.Core.Middleware
  * Provides middleware to log incoming calls to ASP.NET Core
* Sushi.WebserviceLogger.Asmx
  * Provides a SoapExtension to log incoming SOAP calls to ASP.NET ASMX webservices
