using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace System
{
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
        public static string Serialize<TMessage>(this TMessage message)
        {
            return JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            });
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
