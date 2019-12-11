namespace RabbitMQ.EventBus.AspNetCore.Configurations
{
    /// <summary>
    /// 死信交换机
    /// </summary>
    public class DeadLetterExchangeConfig
    {
        /// <summary>
        /// 是否开启(默认开启)
        /// </summary>
        public bool Enabled { set; get; }
        /// <summary>
        /// 交换机名前缀(默认为"dead-")
        /// </summary>
        public string ExchangeNamePrefix { set; get; }
        /// <summary>
        /// 交换机名后缀
        /// </summary>
        public string ExchangeNameSuffix { set; get; }
        /// <summary>
        /// 自定义交换机名(留空则使用原有的交换机名)
        /// </summary>
        public string CustomizeExchangeName { set; get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="enabled">是否开启(默认开启)</param>
        /// <param name="exchangeNamePrefix">交换机名前缀(默认为"dead-")</param>
        /// <param name="exchangeNameSuffix">交换机名后缀</param>
        /// <param name="customizeExchangeName">自定义交换机名(留空则使用原有的交换机名)</param>
        public DeadLetterExchangeConfig(bool enabled = true, string exchangeNamePrefix = "dead-", string exchangeNameSuffix = null, string customizeExchangeName = null)
        {
            Enabled = enabled;
            ExchangeNameSuffix = exchangeNameSuffix;
            ExchangeNamePrefix = exchangeNamePrefix;
            CustomizeExchangeName = customizeExchangeName;
        }
    }
}
