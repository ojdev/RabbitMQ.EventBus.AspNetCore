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
    //public static IServiceCollection AddRabbitMQEventBus(this IServiceCollection services, Func<string> connectionAction, Action<RabbitMQEventBusConnectionConfigurationBuild> eventBusOptionAction)
    //{
    //    RabbitMQEventBusConnectionConfiguration configuration = new();
    //    RabbitMQEventBusConnectionConfigurationBuild configurationBuild = new(configuration);
    //    eventBusOptionAction?.Invoke(configurationBuild);
    //    services.TryAddSingleton<IRabbitMQPersistentConnection>(options =>
    //    {
    //        ILogger<DefaultRabbitMQPersistentConnection> logger = options.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
    //        var connection = new DefaultRabbitMQPersistentConnection(configuration, connectionAction, logger);
    //        connection.TryConnect();
    //        Console.WriteLine("创建一次连接");
    //        return connection;
    //    });
    //    services.TryAddSingleton<IEventHandlerModuleFactory, EventHandlerModuleFactory>();
    //    services.TryAddSingleton<IRabbitMQEventBus, DefaultRabbitMQEventBus>();
    //    foreach (Type mType in typeof(IEvent).GetAssemblies())
    //    {
    //        services.TryAddTransient(mType);
    //        foreach (Type hType in typeof(IEventHandler<>).GetMakeGenericType(mType))
    //        {
    //            services.TryAddTransient(hType);
    //        }
    //    }
    //    return services;
    //}
    /// <summary>
    /// 自动订阅
    /// </summary>
    /// <param name="app"></param>
    //public static void RabbitMQEventBusAutoSubscribe(this IApplicationBuilder app)
    //{
    //    IRabbitMQEventBus eventBus = app.ApplicationServices.GetRequiredService<IRabbitMQEventBus>();
    //    ILogger<IRabbitMQEventBus> logger = app.ApplicationServices.GetRequiredService<ILogger<IRabbitMQEventBus>>();
    //    using (logger.BeginScope("EventBus Subscribe"))
    //    {
    //        logger.LogInformation($"=======================================================================");
    //        foreach (Type mType in typeof(IEvent).GetAssemblies())
    //        {
    //            var handlesAny = typeof(IEventHandler<>).GetMakeGenericType(mType);
    //            if (handlesAny.Any())
    //            {
    //                logger.LogInformation($"{mType.Name}\t=>\t{string.Join("、", handlesAny)}");
    //                eventBus.Subscribe(mType);
    //            }
    //        }
    //        logger.LogInformation($"=======================================================================");
    //    }
    //}
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
        foreach (var handler in responseHandlers)
        {
            rmqeV2.Subscribe(handler.eventType, handler.responseType);
            _logger.LogInformation($"subscribe:\t{handler.eventType}\t=>\t{handler.handlerType}<{handler.eventType.Name},{handler.responseType.Name}>\t return Type : \t{handler.responseType}");
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

