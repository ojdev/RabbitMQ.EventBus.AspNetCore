using Microsoft.Extensions.Logging;
using System;

namespace RabbitMQ.EventBus.AspNetCore.Configurations
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RabbitMQEventBusConnectionConfiguration
    {
        /// <summary>
        /// 
        /// </summary>
        public string ClientProvidedName { get; set; }
        /// <summary>
        /// 连接出现错误后重试连接的次数(默认：50)
        /// </summary>
        public int FailReConnectRetryCount { get; set; }
        /// <summary>
        /// 是否开启网络自动恢复(默认开启)
        /// </summary>
        public bool AutomaticRecoveryEnabled { get; set; }
        /// <summary>
        /// 网络自动恢复时间间隔（默认5秒）
        /// </summary>
        public TimeSpan NetworkRecoveryInterval { get; set; }
        /// <summary>
        /// 消息消费失败的重试时间间隔（默认1秒）
        /// </summary>
        public TimeSpan ConsumerFailRetryInterval { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public LogLevel Level { get; set; }
        /// <summary>
        /// 队列名前缀（默认交换机名）
        /// </summary>
        public QueuePrefixType Prefix { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public RabbitMQEventBusConnectionConfiguration()
        {
            Level = LogLevel.Information;
            FailReConnectRetryCount = 50;
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5);
            AutomaticRecoveryEnabled = true;
            ConsumerFailRetryInterval = TimeSpan.FromSeconds(1);
        }
    }
}
