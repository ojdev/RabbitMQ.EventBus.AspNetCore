namespace RabbitMQ.EventBus.AspNetCore.Configurations;
/// <summary>
/// 队列名前缀
/// </summary>
public enum QueuePrefixType
{
    /// <summary>
    /// 交换机名
    /// </summary>
    ExchangeName,
    /// <summary>
    /// 
    /// </summary>
    ClientProvidedName
}