using RabbitMQ.Client;
using RabbitMQ.EventBus.AspNetCore.Events;
using System;

namespace RabbitMQ.EventBus.AspNetCore
{
    /// <summary>
    /// RabbitMQEventBus
    /// </summary>
    public interface IRabbitMQEventBus
    {
        /// <summary>
        /// 发消息
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="message">消息体</param>
        /// <param name="exchange">交换机</param>
        /// <param name="routingKey">路由</param>
        /// <param name="type">消息类型</param>
        /// <returns></returns>
        void Publish<TMessage>(TMessage message, string exchange, string routingKey, string type = ExchangeType.Topic);
        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <typeparam name="TEvent">消息体</typeparam>
        /// <typeparam name="THandler">消息处理</typeparam>
        /// <param name="type">消息类型</param>
        //void Subscribe<TEvent, THandler>(string type = ExchangeType.Topic)
        //    where TEvent : IEvent
        //    where THandler : IEventHandler<TEvent>;
        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="eventType">消息体</param>
        /// <param name="type">消息类型</param>
        void Subscribe(Type eventType, string type = ExchangeType.Topic);
    }
}
