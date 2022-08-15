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
    public static IServiceProvider AddRabbitMQEventBus(this IServiceCollection services, string endpoint, int port, string username, string password, string visualHost, Action<RabbitMQEventBusConnectionConfigurationBuild> eventBusOptionAction, Action<RabbitMQEventBusModuleOption> moduleOptions = null)
        => AddRabbitMQEventBus(services, () => $"amqp://{username}:{password}@{endpoint}:{port}/{visualHost}", eventBusOptionAction, moduleOptions);

    /// <summary>
    /// 添加RabbitMQEventBus
    /// </summary>
    /// <param name="services"></param>
    /// <param name="connectionAction">使用匿名函数取得连接字符串,用来兼容使用Consul获取服务地址的情况</param>
    /// <param name="eventBusOptionAction"></param>
    /// <returns></returns>
    public static IServiceProvider AddRabbitMQEventBus(this IServiceCollection services, Func<string> connectionAction, Action<RabbitMQEventBusConnectionConfigurationBuild> eventBusOptionAction, Action<RabbitMQEventBusModuleOption> moduleOptions = null)
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
        foreach (Type mType in typeof(IEvent).GetAssemblies())
        {
            foreach (Type hType in typeof(IEventHandler<>).GetMakeGenericType(mType))
            {
                services.TryAddTransient(hType);
            }
        }
        var responseHandlers = services.RegisterEventResponseHandlers().ToList();
        var serviceProvider = services.BuildServiceProvider();
        var _logger = serviceProvider.GetRequiredService<ILogger<DefaultRabbitMQEventBusV2>>();
        var rmqeV2 = serviceProvider.GetService<IRabbitMQEventBus>();
        foreach (var (registerType, handlerType, eventType, responseType) in responseHandlers)
        {
            rmqeV2.Subscribe(eventType, responseType);
            _logger.LogInformation($"subscribe:\t{eventType}\t=>\t{handlerType}<{eventType.Name},{responseType.Name}>\t return Type : \t{responseType}");
        }
        foreach (Type mType in typeof(IEvent).GetAssemblies())
        {
            var handlesAny = typeof(IEventHandler<>).GetMakeGenericType(mType);
            if (handlesAny.Any())
            {
                rmqeV2.Subscribe(mType);
                foreach (var handler in handlesAny)
                {
                    _logger.LogInformation($"subscribe:{mType}\t=>\t{handler}\t");
                }
            }
        }
        return serviceProvider;
    }
}

