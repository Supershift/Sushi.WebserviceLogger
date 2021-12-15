//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.HttpsPolicy;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using Sushi.WebserviceLogger.Core;
//using Sushi.WebserviceLogger.Core.Middleware;
//using Sushi.WebserviceLogger.Filter;
//using Sushi.WebserviceLogger.Persisters;

//namespace Sushi.WebserviceLogger.SampleService
//{
//    public class Startup
//    {
//        public Startup(IConfiguration configuration)
//        {
//            Configuration = configuration;
//        }

//        public IConfiguration Configuration { get; }

//        // This method gets called by the runtime. Use this method to add services to the container.
//        public void ConfigureServices(IServiceCollection services)
//        {   
//            services.AddApplicationInsightsTelemetry();

//            services.AddMvc();

//            //apply settings
//            string elasticUrl = Configuration["ElasticUrl"];
//            string elasticUsername = Configuration["ElasticUsername"];
//            string elasticPassword = Configuration["ElasticPassword"];
//            var elasticSettings = new Nest.ConnectionSettings(new Uri(elasticUrl))
//                    .BasicAuthentication(elasticUsername, elasticPassword)
//                    .ThrowExceptions(true);

//            var elasticClient = new Nest.ElasticClient(elasticSettings);

//            // register elasticClient
//            services.AddSingleton<Nest.IElasticClient>(elasticClient);

//            // register queuepersister instance
//            var queuePersister = new QueuePersister();
//            services.AddSingleton(queuePersister);
//            services.AddSingleton<ILogItemPersister>(queuePersister);

//            //register a background worker to write from queue to elastic
//            services.AddHostedService<QueueProcessorHostedService>();

//            // register logger
//            services.AddTransient(typeof(Sushi.WebserviceLogger.Core.Logger<>));

//            // register filter logging
//            var filterConfig = new MessageLoggerFilterOptions<MyLogItem>();
             
//            services.AddMessageLoggerFilter(filterConfig);         
//        }



//        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
//        public void Configure(IApplicationBuilder app)
//        {
//            app.UseExceptionHandler("/error");
                
//            app.UseHsts();
            
            
//            app.UseHttpsRedirection();
//            app.UseRouting();
//            app.UseStaticFiles();

//            //register message logger middleware            
//            var middlewareConfig = new MessageLoggerConfig<MyLogItem>();
//            middlewareConfig.AddLogItemCallback += (MyLogItem logItem, HttpContext context) =>
//            {
//                logItem.MyKeyword = "my value";
//                return logItem;
//            };
//            middlewareConfig.IndexNameCallback = () => "webservicelogs_test";
//            //app.UseMiddleware<MessageLogger<LogItem>>(loggingConfig);
//            //app.UseMessageLogger<LogItem>(loggingConfig);
//            //app.UseWhen(x => x.Request.Path.Value?.StartsWith("/api") == true, a => a.UseMessageLogger<LogItem>(loggingConfig));

//            app.UseWhen(x => x.Request.Path.Value?.StartsWith("/api") == true, a => a.UseMessageLogger(middlewareConfig));

//            var mockPersister = new MockPersister();
//            var mockMiddlewareConfig = new MessageLoggerConfig<LogItem>();            
//            app.UseWhen(x => x.Request.Path.Value?.StartsWith("/mock") == true, a => a.UseMessageLogger(mockMiddlewareConfig));

//            app.MapControllers();
//        }
//    }
//}
