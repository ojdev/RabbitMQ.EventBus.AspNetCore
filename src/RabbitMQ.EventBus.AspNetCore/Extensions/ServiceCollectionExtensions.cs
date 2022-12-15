using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
/// <summary>
/// 
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加RabbitMQEventBus
    /// </summary>
    /// <param name="services"></param>
    /// <param name="endpoint"></param>
    /// <param name="port"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="visualHost"></param>
    /// <param name="eventBusOptionAction"></param>
    /// <param name="moduleOptions"></param>
    /// <returns></returns>
    public static IServiceCollection AddRabbitMQEventBus(this IServiceCollection services, string endpoint, int port, string username, string password, string visualHost, Action<RabbitMQEventBusConnectionConfigurationBuild> eventBusOptionAction, Action<RabbitMQEventBusModuleOption> moduleOptions = null)
        => AddRabbitMQEventBus(services, () => $"amqp://{username}:{password}@{endpoint}:{port}/{visualHost}", eventBusOptionAction, moduleOptions);

    /// <summary>
    /// 添加RabbitMQEventBus
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionAction">使用匿名函数取得连接字符串,用来兼容使用Consul获取服务地址的情况</param>
    /// <param name="eventBusOptionAction"></param>
    /// <returns></returns>
    public static IServiceCollection AddRabbitMQEventBus(this IServiceCollection services, Func<string> connectionAction, Action<RabbitMQEventBusConnectionConfigurationBuild> eventBusOptionAction, Action<RabbitMQEventBusModuleOption> moduleOptions = null)
    {
        RabbitMQEventBusConnectionConfiguration configuration = new();
        RabbitMQEventBusConnectionConfigurationBuild configurationBuild = new(configuration);
        eventBusOptionAction?.Invoke(configurationBuild);
        services.TryAddSingleton<IRabbitMQPersistentConnection>(options =>
        {
            ILogger<DefaultRabbitMQPersistentConnection> logger = options.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
            var connection = DefaultRabbitMQPersistentConnection.CreateInstance(configuration, connectionAction, logger);
            connection.TryConnect();
            logger.LogInformation("RabbitMQ event bus connected.");
            return connection;
        });

        services.TryAddSingleton<IRabbitMQEventBus>(options =>
        {
            IRabbitMQPersistentConnection rabbitMQPersistentConnection = options.GetRequiredService<IRabbitMQPersistentConnection>();
            ILogger<DefaultRabbitMQEventBusV2> logger = options.GetRequiredService<ILogger<DefaultRabbitMQEventBusV2>>();
            var eventBus = DefaultRabbitMQEventBusV2.CreateInstance(rabbitMQPersistentConnection, options, logger);
            return eventBus;
        });
        foreach (var (registerType, handlerType, eventType, responseType) in RabbitmqEventBusHandlers.RegisterEventResponseHandlers())
        {
            services.TryAddTransient(registerType, handlerType);
        }
        foreach (var (handlerType, eventType) in RabbitmqEventBusHandlers.RegisterEventHandlers())
        {
            services.TryAddTransient(handlerType);
        }
        return services;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="app"></param>
    public static void UseRabbitmqEventBus(this IApplicationBuilder app)
    {
        IRabbitMQEventBus rmqeV2 = app.ApplicationServices.GetRequiredService<IRabbitMQEventBus>();
        var _logger = app.ApplicationServices.GetRequiredService<ILogger<DefaultRabbitMQEventBusV2>>();
        foreach (var (registerType, handlerType, eventType, responseType) in RabbitmqEventBusHandlers.RegisterEventResponseHandlers())
        {
            rmqeV2.Subscribe(eventType, responseType);
            _logger.LogInformation($"subscribe:\t{eventType}\t=>\t{handlerType}<{eventType.Name},{responseType.Name}>\t return Type : \t{responseType}");
        }
        foreach (var (handlerType, eventType) in RabbitmqEventBusHandlers.RegisterEventHandlers())
        {
            rmqeV2.Subscribe(eventType);
            _logger.LogInformation($"subscribe:\t{eventType}\t=>\t{handlerType}<{eventType.Name}>");
        }
    }
}

