using System.Threading.Tasks;

namespace RabbitMQ.EventBus.AspNetCore.Modules
{
    /// <summary>
    /// 模块
    /// </summary>
    public interface IModuleHandle
    {
        /// <summary>
        /// 发消息
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        Task PublishEvent(EventBusArgs e);
        /// <summary>
        /// 收消息
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        Task SubscribeEvent(EventBusArgs e);
    }
}
