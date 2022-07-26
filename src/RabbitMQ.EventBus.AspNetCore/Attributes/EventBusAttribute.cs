namespace RabbitMQ.EventBus.AspNetCore.Attributes;
/// <summary>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class EventBusAttribute : Attribute
{
    /// <summary>
    /// 队列名
    /// </summary>
    public string Queue { get; set; }
    /// <summary>
    /// 交换机名
    /// </summary>
    public string Exchange { get; set; }
    /// <summary>
    /// 路由Key
    /// </summary>
    public string RoutingKey { get; set; }
    /// <summary>
    /// Configures QoS parameters of the Basic content-class.
    /// </summary>
    public ushort? BasicQos { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public EventBusAttribute()
    {
        RoutingKey = "";
    }
}