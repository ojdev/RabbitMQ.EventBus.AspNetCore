using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;

namespace RabbitMQ.EventBus.AspNetCore.Simple
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
            string assemblyName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddControllers();
            services.AddHealthChecks();

            services.AddRabbitMQEventBus(() => "amqp://guest:guest@localhost:5672/", eventBusOptionAction: eventBusOption =>
             {
                 eventBusOption.ClientProvidedAssembly(assemblyName);
                 eventBusOption.EnableRetryOnFailure(true, 5000, TimeSpan.FromSeconds(30));
                 eventBusOption.RetryOnFailure(TimeSpan.FromSeconds(1));
                 eventBusOption.MessageTTL(2000);
                 eventBusOption.SetBasicQos(10);
                 eventBusOption.DeadLetterExchangeConfig(config =>
                 {
                     config.Enabled = true;
                     config.ExchangeNameSuffix = "-test";
                 });
             });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.RabbitMQEventBusAutoSubscribe();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/hc", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    ResultStatusCodes = {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status200OK,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    }
                });
            });
        }
    }
}
