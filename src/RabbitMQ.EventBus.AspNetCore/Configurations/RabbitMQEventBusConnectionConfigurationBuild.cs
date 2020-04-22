using Microsoft.Extensions.Logging;
using System;

namespace RabbitMQ.EventBus.AspNetCore.Configurations
{
    /// <summary>
    /// 
    /// </summary>
    public class RabbitMQEventBusConnectionConfigurationBuild
    {
        private RabbitMQEventBusConnectionConfiguration Configuration { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public RabbitMQEventBusConnectionConfigurationBuild(RabbitMQEventBusConnectionConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// 设置客户端名称
        /// </summary>
        /// <param name="assembly"></param>
        public void ClientProvidedAssembly(string assembly)
        {
            Configuration.ClientProvidedName = assembly;
        }
        /// <summary>
        /// 网络恢复配置
        /// </summary>
        /// <param name="automaticRecovery">是否开启网络自动恢复（默认：true）</param>
        /// <param name="maxRetryCount">连接出现错误后重试连接的指数次数(默认：50)</param>
        /// <param name="maxRetryDelay">网络自动恢复时间间隔（默认5秒）</param>
        public void EnableRetryOnFailure(bool automaticRecovery, int maxRetryCount, TimeSpan maxRetryDelay)
        {
            Configuration.AutomaticRecoveryEnabled = automaticRecovery;
            Configuration.FailReConnectRetryCount = maxRetryCount;
            Configuration.NetworkRecoveryInterval = maxRetryDelay;
        }
        /// <summary>
        /// 重试错误的消息
        /// </summary>
        /// <param name="maxRetryDelay">消息失败的重试时间间隔（默认1秒）</param>
        public void RetryOnFailure(TimeSpan maxRetryDelay)
        {
            Configuration.ConsumerFailRetryInterval = maxRetryDelay;
        }
        /// <summary>
        /// 设置日志输出级别
        /// </summary>
        /// <param name="level">日志级别</param>
        public void LoggingWriteLevel(LogLevel level)
        {
            Configuration.Level = level;
        }
        /// <summary>
        /// 队列名前缀
        /// </summary>
        /// <param name="queuePrefix"><see cref="QueuePrefixType"/></param>
        public void QueuePrefix(QueuePrefixType queuePrefix = QueuePrefixType.ClientProvidedName)
        {
            Configuration.Prefix = queuePrefix;
        }
        /// <summary>
        /// 设置消息的驻留时常(毫秒)
        /// 如果开启了死信队列设置则默认为60000毫秒
        /// </summary>
        /// <param name="millisecond"></param>
        public void MessageTTL(int millisecond)
        {
            Configuration.MessageTTL = millisecond;
        }
        /// <summary>
        /// 死信队列设置
        /// </summary>
        /// <param name="config"></param>
        public void DeadLetterExchangeConfig(Action<DeadLetterExchangeConfig> config)
        {
            config?.Invoke(Configuration.DeadLetterExchange);
            if (Configuration.DeadLetterExchange.Enabled && Configuration.MessageTTL == null)
            {
                Configuration.MessageTTL = 60000;
            }
        }
        /// <summary>
        /// 设置预取条数
        /// </summary>
        /// <param name="prefetchCount"></param>
        public void SetBasicQos(ushort prefetchCount)
        {
            if (prefetchCount < 1)
            {
                throw new ArgumentOutOfRangeException($"{nameof(prefetchCount)}必须大于0");
            }
            Configuration.PrefetchCount = prefetchCount;
        }
    }
}
