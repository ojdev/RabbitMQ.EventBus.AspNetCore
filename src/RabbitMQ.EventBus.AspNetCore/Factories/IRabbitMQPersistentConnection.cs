namespace RabbitMQ.EventBus.AspNetCore.Factories;
/// <summary>
/// 
/// </summary>
public interface IRabbitMQPersistentConnection : IDisposable
{
    /// <summary>
    /// 
    /// </summary>
    RabbitMQEventBusConnectionConfiguration Configuration { get; }
    /// <summary>
    /// 连接点
    /// </summary>
    string Endpoint { get; }
    /// <summary>
    /// 连接是否打开
    /// </summary>
    bool IsConnected { get; }
    /// <summary>
    /// 尝试连接
    /// </summary>
    /// <returns></returns>
    bool TryConnect();
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IModel CreateModel();
}

public static class RabbitmqEventBusHandlers
{

    public static IEnumerable<(Type registerType, Type handlerType, Type eventType, Type responseType)> RegisterEventResponseHandlers()
    {
        foreach (var eventResponseHandler in AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes()).Where(t => t.GetInterfaces().Any(x => x.IsGenericType && !x.IsGenericTypeDefinition && x.GetGenericTypeDefinition() == typeof(IEventResponseHandler<,>))))
        {
            var interfaces = eventResponseHandler.GetInterfaces()
                .Where(x =>
                        x.IsGenericType &&
                        !x.IsGenericTypeDefinition &&
                        x.GetGenericTypeDefinition() == typeof(IEventResponseHandler<,>)
                        );
            foreach (var iface in interfaces)
            {
                var eventResponseHandleArgs = iface.GetGenericArguments();
                var eventType = eventResponseHandleArgs[0];
                var responseType = eventResponseHandleArgs[1];
                yield return (iface, eventResponseHandler, eventType, responseType);
            }
        }
    }


    public static IEnumerable<(Type handlerType, Type eventType)> RegisterEventHandlers()
    {
        foreach (Type mType in typeof(IEvent).GetAssemblies())
        {
            foreach (Type hType in typeof(IEventHandler<>).GetMakeGenericType(mType))
            {
                yield return (hType, mType);
            }
        }

        //foreach (var eventHandler in AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes())
        //    .Where(t => t.GetInterfaces().Any(x => x.IsGenericType && !x.IsGenericTypeDefinition && x.GetGenericTypeDefinition() == typeof(IEventHandler<>))))
        //{
        //    var interfaces = eventHandler.GetInterfaces()
        //        .Where(x =>
        //                x.IsGenericType &&
        //                !x.IsGenericTypeDefinition &&
        //                x.GetGenericTypeDefinition() == typeof(IEventHandler<>)
        //                );
        //    foreach (var iface in interfaces)
        //    {
        //        var eventType = iface.GetGenericArguments().FirstOrDefault();
        //        services.TryAddTransient(eventType);
        //        services.TryAddTransient(typeof(IEventHandler<>).MakeGenericType(eventType), eventHandler);
        //        yield return (iface, eventHandler, eventType);
        //    }
        //}
    }
}