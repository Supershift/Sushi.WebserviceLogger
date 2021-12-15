using Sushi.WebserviceLogger.Core;
using Sushi.WebserviceLogger.Core.Middleware;
using Sushi.WebserviceLogger.Filter;
using Sushi.WebserviceLogger.Persisters;
using Sushi.WebserviceLogger.SampleService;

var builder = WebApplication.CreateBuilder(args);

// set configuration
var Configuration = builder.Configuration;
var services = builder.Services;

services.AddApplicationInsightsTelemetry();

services.AddControllers();

//apply settings
string elasticUrl = Configuration["ElasticUrl"];
string elasticUsername = Configuration["ElasticUsername"];
string elasticPassword = Configuration["ElasticPassword"];
var elasticSettings = new Nest.ConnectionSettings(new Uri(elasticUrl))
        .BasicAuthentication(elasticUsername, elasticPassword)
        .ThrowExceptions(true);

var elasticClient = new Nest.ElasticClient(elasticSettings);

// register elasticClient
services.AddSingleton<Nest.IElasticClient>(elasticClient);

// register queuepersister instance
var queuePersister = new QueuePersister();
services.AddSingleton(queuePersister);
services.AddSingleton<ILogItemPersister>(queuePersister);

//register a background worker to write from queue to elastic
services.AddHostedService<QueueProcessorHostedService>();

// register logger
services.AddTransient(typeof(Sushi.WebserviceLogger.Core.Logger<>));

// register filter logging
var filterConfig = new MessageLoggerFilterOptions<MyLogItem>();

services.AddMessageLoggerFilter(filterConfig);

// Configure the HTTP request pipeline.
var app = builder.Build();
app.UseExceptionHandler("/error");

app.UseHsts();


app.UseHttpsRedirection();
app.UseRouting();
app.UseStaticFiles();

//register message logger middleware            
var middlewareConfig = new MessageLoggerConfig<MyLogItem>();
middlewareConfig.AddLogItemCallback += (MyLogItem logItem, HttpContext context) =>
{
    logItem.MyKeyword = "my value";
    return logItem;
};
middlewareConfig.IndexNameCallback = () => "webservicelogs_test";
//app.UseMiddleware<MessageLogger<LogItem>>(loggingConfig);
//app.UseMessageLogger<LogItem>(loggingConfig);
//app.UseWhen(x => x.Request.Path.Value?.StartsWith("/api") == true, a => a.UseMessageLogger<LogItem>(loggingConfig));

app.UseWhen(x => x.Request.Path.Value?.StartsWith("/api") == true, a => a.UseMessageLogger(middlewareConfig));

var mockPersister = new MockPersister();
var mockMiddlewareConfig = new MessageLoggerConfig<LogItem>();
app.UseWhen(x => x.Request.Path.Value?.StartsWith("/mock") == true, a => a.UseMessageLogger(mockMiddlewareConfig));

app.MapControllers();

app.Run();