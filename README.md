# Sushi.WebserviceLogger
[![Build Status](https://dev.azure.com/supershift/Mediakiwi/_apis/build/status/Sushi.WebserviceLogger?branchName=main)](https://dev.azure.com/supershift/Mediakiwi/_build/latest?definitionId=100&branchName=main)
[![NuGet version (Sushi.WebserviceLogger.Core)](https://img.shields.io/nuget/v/Sushi.WebserviceLogger.Core.svg?style=flat-square)](https://www.nuget.org/packages/Sushi.WebserviceLogger.Core/)
## Features
Sushi WebserviceLogger is a set of NuGet libraries that allows you to easily log traffic from and to webservices into Elasticsearch.
## Requirements
* ElasticSearch 7.14 or higher
* .Net 6
## Quick start
### Log outgoing traffic
```csharp
// register a persister
service.AddInProcessPersister(new Nest.ElasticClient());

// configure your HttpClient to use logging
services
  .AddHttpClient<MyConnector>(client =>
  {
      // configure client here
      client.BaseAddress = new Uri("https://www.contosco.com");
  })
  .AddWebserviceLogging<LogItem>(o =>
  {
      // configure logging here
  });

// then inject the client into your class
public class MyConnector
{
  private readonly HttpClient _httpClient;
  public MyConnector(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }

  public async GetAsync()
  {    
    await _httpClient.GetAsync("/myResource/12");
  }
}
### Log incoming traffic
```csharp
// register a persister
service.AddInProcessPersister(new Nest.ElasticClient());

// register filter logging
services.AddWebserviceLoggerFilter<LogItem>(c => 
{
  // configure logging here
});

// decorate your controller with a filter attribute
[ServiceFilter(typeof(Filter.WebserviceLoggerFilter<LogItem>))]
public class SampleController : ControllerBase
{
    [HttpGet]
    [Route("ping")]        
    public ActionResult Ping()
    {
        return Ok("hello world");
    }
}
```

## More reading
You can find more info in the projects MD files:
- [Persisters](/src/Sushi.WebserviceLogger.Persisters/README.md)
- [Client Factory](/src/Sushi.Elastic.ClientFactory/README.md)