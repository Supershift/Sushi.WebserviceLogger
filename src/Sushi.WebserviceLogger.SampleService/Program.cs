using Sushi.WebserviceLogger.Core;
using Sushi.WebserviceLogger.Filter;
using Sushi.WebserviceLogger.Middleware;
using Sushi.WebserviceLogger.Persisters;
using Sushi.WebserviceLogger.SampleService;

var builder = WebApplication.CreateBuilder(args);

// set configuration
var Configuration = builder.Configuration;
var services = builder.Services;

services.AddApplicationInsightsTelemetry();

services.AddControllers();

// register persister instance
services.AddMockPersister(o=>o.Callback = (logItem, index) => 
{ 
    Console.WriteLine($"Received logitem: {logItem.Id}"); 
});

// register a background worker to write from queue to elastic
//var inMemoryClient = new Nest.ElasticClient(new Nest.ConnectionSettings(new Elasticsearch.Net.InMemoryConnection()));
//services.AddQueuePersister(() => inMemoryClient);

// register filter logging
services.AddWebserviceLoggerFilter<LogItem>();

// Configure the HTTP request pipeline.
var app = builder.Build();
app.UseExceptionHandler("/error");

app.UseHsts();

app.UseHttpsRedirection();
app.UseRouting();
app.UseStaticFiles();
app.MapControllers();

app.Run();