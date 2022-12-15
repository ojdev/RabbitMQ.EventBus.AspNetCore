using Microsoft.Extensions.DependencyInjection.Extensions;
using RabbitMQ.EventBus.AspNetCore.Events;
using System;
using System.Data;

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

    public static bool IsNullOrWhiteSpace(this string source)
    {
        return string.IsNullOrWhiteSpace(source);
    }
}