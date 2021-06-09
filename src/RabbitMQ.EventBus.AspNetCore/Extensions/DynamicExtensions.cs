using Newtonsoft.Json;
using System.Text;

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
            return JsonConvert.SerializeObject(message);
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
