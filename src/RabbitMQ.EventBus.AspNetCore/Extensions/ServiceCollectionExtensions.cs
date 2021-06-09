using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.EventBus.AspNetCore;
using RabbitMQ.EventBus.AspNetCore.Configurations;
using RabbitMQ.EventBus.AspNetCore.Events;
using RabbitMQ.EventBus.AspNetCore.Factories;
using RabbitMQ.EventBus.AspNetCore.Modules;
using System;
using System.Linq;

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
        /// <param name="connectionAction">使用匿名函数取得连接字符串,用来兼容使用Consul获取服务地址的情况</param>
        /// <param name="eventBusOptionAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMQEventBus(this IServiceCollection services, Func<string> connectionAction, Action<RabbitMQEventBusConnectionConfigurationBuild> eventBusOptionAction)
        {
            RabbitMQEventBusConnectionConfiguration configuration = new();
            RabbitMQEventBusConnectionConfigurationBuild configurationBuild = new(configuration);
            eventBusOptionAction?.Invoke(configurationBuild);
            services.TryAddSingleton<IRabbitMQPersistentConnection>(options =>
            {
                ILogger<DefaultRabbitMQPersistentConnection> logger = options.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
                var connection = new DefaultRabbitMQPersistentConnection(configuration, connectionAction, logger);
                connection.TryConnect();
                return connection;
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
                logger.LogInformation($"=======================================================================");
                foreach (Type mType in typeof(IEvent).GetAssemblies())
                {
                    var handlesAny = typeof(IEventHandler<>).GetMakeGenericType(mType);
                    if (handlesAny.Any())
                    {
                        logger.LogInformation($"{mType.Name}\t=>\t{string.Join("、", handlesAny)}");
                        eventBus.Subscribe(mType);
                    }
                }
                logger.LogInformation($"=======================================================================");
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
            RabbitMQEventBusModuleOption moduleOption = new(factory, app.ApplicationServices);
            moduleOptions?.Invoke(moduleOption);
        }
    }
}
