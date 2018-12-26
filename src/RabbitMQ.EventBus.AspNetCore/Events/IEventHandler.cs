using RabbitMQ.Client.Events;
using System;
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
        Task Handle(TEvent message);
    }
    /// <summary>
    /// 
    /// </summary>
    public class MessageEventArgs
    {
        /// <summary>
        /// 原始消息
        /// </summary>
        public string Original { get; }
        /// <summary>
        /// 是否重新投递的消息
        /// </summary>
        public bool Redelivered { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="original"></param>
        /// <param name="redelivered"></param>
        public MessageEventArgs(string original, bool redelivered)
        {
            Original = original ?? throw new ArgumentNullException(nameof(original));
            Redelivered = redelivered;
        }
    }
}
