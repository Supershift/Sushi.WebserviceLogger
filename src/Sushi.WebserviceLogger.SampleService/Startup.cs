using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sushi.WebserviceLogger.Core;
using Sushi.WebserviceLogger.Core.Middleware;
using Sushi.WebserviceLogger.Persisters;

namespace Sushi.WebserviceLogger.SampleService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {   
            services.AddApplicationInsightsTelemetry();

            services.AddMvc();

            //apply settings
            string elasticUrl = Configuration["ElasticUrl"];
            string elasticUsername = Configuration["ElasticUsername"];
            string elasticPassword = Configuration["ElasticPassword"];
            ElasticConfig = new Core.ElasticConfiguration(elasticUrl, elasticUsername, elasticPassword);          
            
            //create DI for queuepersister
            services.AddSingleton(s=> new QueuePersister(ElasticConfig));
            //register a background worker to write from queue to elastic
            services.AddHostedService<QueueProcessorHostedService>();            
        }

        private ElasticConfiguration ElasticConfig { get; set; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, QueuePersister persister)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            
            app.UseHttpsRedirection();
            app.UseRouting();

            //register message logger middleware            
            var middlewareConfig = new MessageLoggerConfig<MyLogItem>(persister);
            middlewareConfig.AddLogItemCallback += (MyLogItem logItem, HttpContext context) =>
            {
                logItem.MyKeyword = "my value";
                return logItem;
            };
            middlewareConfig.IndexNameCallback = dt => "webservicelogs_test";
            //app.UseMiddleware<MessageLogger<LogItem>>(loggingConfig);
            //app.UseMessageLogger<LogItem>(loggingConfig);
            //app.UseWhen(x => x.Request.Path.Value?.StartsWith("/api") == true, a => a.UseMessageLogger<LogItem>(loggingConfig));

            app.UseWhen(x => x.Request.Path.Value?.StartsWith("/api") == true, a => a.UseMessageLogger(middlewareConfig));


            var mockPersister = new MockPersister();
            var mockMiddlewareConfig = new MessageLoggerConfig<LogItem>(mockPersister);            
            app.UseWhen(x => x.Request.Path.Value?.StartsWith("/mock") == true, a => a.UseMessageLogger(mockMiddlewareConfig));

            //registere MVC middleware (executes Web API)
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
