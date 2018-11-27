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
        [Obsolete("由于可能造成阻塞，暂时停用")]
        public void RetryOnFailure(TimeSpan maxRetryDelay)
        {
            Configuration.ConsumerFailRetryInterval = maxRetryDelay;
        }
    }
}
