namespace Microsoft.Extensions.DependencyInjection;
/// <summary>
/// 
/// </summary>
internal static class TypeExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetAssemblies(this Type type)
    {
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(type)));
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="interfalceType"></param>
    /// <param name="makeType"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetMakeGenericType(this Type interfalceType, Type makeType)
    {
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(interfalceType.MakeGenericType(makeType))));
    }
    public static IEnumerable<(Type registerType, Type handlerType, Type eventType, Type responseType)> RegisterEventResponseHandlers(this IServiceCollection services)
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
                services.TryAddTransient(iface, eventResponseHandler);
                yield return (iface, eventResponseHandler, eventType, responseType);
            }
        }
    }
    public static bool IsNullOrWhiteSpace(this string source)
    {
        return string.IsNullOrWhiteSpace(source);
    }
}