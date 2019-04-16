using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddRabbitMQEventBus(() => "amqp://guest:guest@192.168.0.252:5672/", eventBusOptionAction: eventBusOption =>
             {
                 eventBusOption.ClientProvidedAssembly(assemblyName);
                 eventBusOption.EnableRetryOnFailure(true, 5000, TimeSpan.FromSeconds(30));
                 eventBusOption.RetryOnFailure(TimeSpan.FromSeconds(1));
             });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.RabbitMQEventBusAutoSubscribe();
            app.UseMvc();
        }
    }
}
