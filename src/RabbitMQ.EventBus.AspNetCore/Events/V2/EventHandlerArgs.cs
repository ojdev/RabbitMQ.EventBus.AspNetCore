namespace RabbitMQ.EventBus.AspNetCore.Events;

public class HandlerEventArgs<TEvent>
{
    /// <summary>
    /// Contains all the information about a message delivered from an AMQP broker within<br />
    /// the Basic content-class.
    /// </summary>
    public BasicDeliverEventArgs EventArgs { get; }
    /// <summary>
    /// 原始消息
    /// </summary>
    public string Original { get; }
    private TEvent _event;
    /// <summary>
    /// 序列化后的对象
    /// </summary>
    public TEvent EventObject
    {
        get
        {
            if (_event == null)
            {
                _event = Original.Deserialize<TEvent>();
            }
            return _event;
        }
    }

    public HandlerEventArgs(string original, BasicDeliverEventArgs eventArgs)
    {
        Original = original ?? throw new ArgumentNullException(nameof(original));
        EventArgs = eventArgs ?? throw new ArgumentNullException(nameof(eventArgs));
    }
}