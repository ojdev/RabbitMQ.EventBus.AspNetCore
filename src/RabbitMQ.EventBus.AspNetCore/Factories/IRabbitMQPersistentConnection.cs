using RabbitMQ.Client;
using RabbitMQ.EventBus.AspNetCore.Configurations;
using System;

namespace RabbitMQ.EventBus.AspNetCore.Factories
{
    /// <summary>
    /// 
    /// </summary>
    public interface IRabbitMQPersistentConnection : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        RabbitMQEventBusConnectionConfiguration Configuration { get; }
        /// <summary>
        /// 连接点
        /// </summary>
        string Endpoint { get; }
        /// <summary>
        /// 是否打开链接
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// 尝试链接
        /// </summary>
        /// <returns></returns>
        bool TryConnect();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IModel CreateModel();
    }
}