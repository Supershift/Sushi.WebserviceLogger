# Sushi.WebserviceLogger.Asmx
[![Build Status](https://dev.azure.com/supershift/Mediakiwi/_apis/build/status/Sushi.WebserviceLogger?branchName=main)](https://dev.azure.com/supershift/Mediakiwi/_build/latest?definitionId=100&branchName=main)
[![NuGet version (Sushi.WebserviceLogger.Asmx)](https://img.shields.io/nuget/v/Sushi.WebserviceLogger.Asmx.svg?style=flat-square)](https://www.nuget.org/packages/Sushi.WebserviceLogger.Asmx/)
## Features
Provides a SoapExtension to log incoming SOAP calls to ASP.NET ASMX webservices.
## Quick start
Create an instance of Logger at the startup of your application, for instance in Global.asax.
```csharp
var elasticConfig = new Core.ElasticConfiguration("elasticUrl", "elasticUsername", "elasticPassword");
var persister = new InProcessPersister(elasticConfig);
// make this instance accessible to other threads
var logger = new Logger<LogItem>(persister)
{
    IndexNameCallback = (dt) => $"asmxlogs_{dt:yyyy-MM}"
};
```
Create an implementation of the abstract class ServerTracer<T>. The implementation cannot be a generic class. The implementation will provide:
* The type to use as logging object (default is LogItem)
* The instance of Logger to use, with its Elastic Configuration
```csharp
public class SampleAsmxServerTracer : ServerTracer<LogItem>
{
    public override Logger<LogItem> Logger { get; set; }

    public override void Initialize(object initializer)
    {
        Logger = Config.AsmxLogger;
    }
}
```
Register the SampleAsmxTracer as SoapExtension in the web.config
```xml
<system.web>    
  <webServices>
    <soapExtensionTypes>              
      <add type="Sushi.WebserviceLogger.SampleService.SampleAsmxServerTracer, Sushi.WebserviceLogger.SampleService" priority="1" group="High" />
    </soapExtensionTypes>
  </webServices>
</system.web>
```
