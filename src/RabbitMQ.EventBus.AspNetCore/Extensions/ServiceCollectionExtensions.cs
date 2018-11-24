using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.EventBus.AspNetCore;
using RabbitMQ.EventBus.AspNetCore.Configurations;
using RabbitMQ.EventBus.AspNetCore.Events;
using RabbitMQ.EventBus.AspNetCore.Factories;
using RabbitMQ.EventBus.AspNetCore.Modules;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加RabbitMQEventBus
        /// </summary>
        /// <param name="services"></param>
        /// <param name="connectionString"></param>
        /// <param name="eventBusOptionAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMQEventBus(this IServiceCollection services, string connectionString, Action<RabbitMQEventBusConnectionConfigurationBuild> eventBusOptionAction)
        {
            RabbitMQEventBusConnectionConfiguration configuration = new RabbitMQEventBusConnectionConfiguration();
            RabbitMQEventBusConnectionConfigurationBuild configurationBuild = new RabbitMQEventBusConnectionConfigurationBuild(configuration);
            eventBusOptionAction?.Invoke(configurationBuild);
            services.TryAddSingleton<IRabbitMQPersistentConnection>(options =>
            {
                ILogger<DefaultRabbitMQPersistentConnection> logger = options.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
                IConnectionFactory factory = new ConnectionFactory
                {
                    AutomaticRecoveryEnabled = configuration.AutomaticRecoveryEnabled,
                    NetworkRecoveryInterval = configuration.NetworkRecoveryInterval,
                    Uri = new Uri(connectionString),
                };
                return new DefaultRabbitMQPersistentConnection(configuration, factory, logger);
            });
            services.TryAddSingleton<IEventHandlerModuleFactory, EventHandlerModuleFactory>();
            services.TryAddSingleton<IRabbitMQEventBus, DefaultRabbitMQEventBus>();
            foreach (Type mType in typeof(IEvent).GetAssemblies())
            {
                services.TryAddTransient(mType);
                foreach (Type hType in typeof(IEventHandler<>).GetMakeGenericType(mType))
                {
                    services.TryAddTransient(hType);
                }
            }
            return services;
        }
        /// <summary>
        /// 自动订阅
        /// </summary>
        /// <param name="app"></param>
        public static void RabbitMQEventBusAutoSubscribe(this IApplicationBuilder app)
        {
            IRabbitMQEventBus eventBus = app.ApplicationServices.GetRequiredService<IRabbitMQEventBus>();
            ILogger<IRabbitMQEventBus> logger = app.ApplicationServices.GetRequiredService<ILogger<IRabbitMQEventBus>>();
            using (logger.BeginScope("EventBus Subscribe"))
            {
                foreach (Type mType in typeof(IEvent).GetAssemblies())
                {
                    logger.LogInformation($"{mType.Name}");
                    foreach (Type hType in typeof(IEventHandler<>).GetMakeGenericType(mType))
                    {
                        logger.LogInformation($"{mType.Name}=>{hType.Name}");
                        eventBus.Subscribe(mType, hType);
                    }
                }
                logger.LogInformation($"Ok.");
            }
        }
        /// <summary>
        /// 添加模块
        /// </summary>
        /// <param name="app"></param>
        /// <param name="moduleOptions"></param>
        public static void RabbitMQEventBusModule(this IApplicationBuilder app, Action<RabbitMQEventBusModuleOption> moduleOptions)
        {
            IEventHandlerModuleFactory factory = app.ApplicationServices.GetRequiredService<IEventHandlerModuleFactory>();
            RabbitMQEventBusModuleOption moduleOption = new RabbitMQEventBusModuleOption(factory);
            moduleOptions?.Invoke(moduleOption);
        }
    }
}
