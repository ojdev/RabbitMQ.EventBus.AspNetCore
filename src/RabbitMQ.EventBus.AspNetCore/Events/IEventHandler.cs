using System.Threading.Tasks;

namespace RabbitMQ.EventBus.AspNetCore.Events
{
    /// <summary>
    /// EventBus消息处理
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEventHandler<TEvent> where TEvent : IEvent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Task Handle(TEvent message, EventHandlerArgs args);
    }
    /// <summary>
    /// 
    /// </summary>
    public class EventHandlerArgs
    {
        /// <summary>
        /// 原始消息
        /// </summary>
        public string Original { get; set; }
        /// <summary>
        /// 是否为打回的消息
        /// </summary>
        public bool Redelivered { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Exchange { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string RoutingKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="original"></param>
        /// <param name="redelivered"></param>
        /// <param name="exchange"></param>
        /// <param name="routingKey"></param>
        public EventHandlerArgs(string original, bool redelivered, string exchange, string routingKey)
        {
            Original = original;
            Redelivered = redelivered;
            Exchange = exchange;
            RoutingKey = routingKey;
        }
    }
}
