using System.Text;
using System.Text.Json;

namespace System
{
    /// <summary>
    /// 
    /// </summary>
    public static class DynamicExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string Serialize<TMessage>(this TMessage message)
        {
            return JsonSerializer.Serialize(message);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static byte[] GetBytes(this string body)
        {
            return Encoding.UTF8.GetBytes(body);
        }
    }
}
