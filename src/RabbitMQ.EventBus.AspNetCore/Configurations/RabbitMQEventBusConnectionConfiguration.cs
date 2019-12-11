using Microsoft.Extensions.Logging;
using System;

namespace RabbitMQ.EventBus.AspNetCore.Configurations
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RabbitMQEventBusConnectionConfiguration
    {
        private int? messageTTL;

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
        /// 队列名前缀（默认ClientProvidedName）
        /// </summary>
        public QueuePrefixType Prefix { get; set; }
        /// <summary>
        /// 死信交换机设置
        /// </summary>
        public DeadLetterExchangeConfig DeadLetterExchange { set; get; }
        /// <summary>
        /// 消息驻留时长(毫秒),超过此市场的则判断为死信
        /// </summary>
        public int? MessageTTL
        {
            get => messageTTL;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException($"{nameof(MessageTTL)}必须大于0");
                }
                messageTTL = value;
            }
        }
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
            DeadLetterExchange = new DeadLetterExchangeConfig();
        }
    }
}
