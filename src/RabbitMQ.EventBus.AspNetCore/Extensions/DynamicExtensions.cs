namespace RabbitMQ.EventBus.AspNetCore.Extensions;

/// <summary>
/// 
/// </summary>
internal static class DynamicExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="message"></param>
    /// <returns></returns>
    public static string Serialize<TMessage>(this TMessage message) => JsonSerializer.Serialize(message, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    });
    /// <summary>
    /// 
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public static byte[] GetBytes(this string body)
    {
        return Encoding.UTF8.GetBytes(body);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="message"></param>
    /// <returns></returns>
    public static TResponse Deserialize<TResponse>(this string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            try
            {
                return JsonSerializer.Deserialize<TResponse>(message, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                });
            }
            catch
            {
                try
                {
                    return (TResponse)TypeDescriptor.GetConverter(typeof(TResponse)).ConvertFromInvariantString(message);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        return default;
    }
}