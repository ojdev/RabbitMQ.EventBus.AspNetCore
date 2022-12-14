namespace RabbitMQ.EventBus.AspNetCore.Extensions;
/// <summary>
/// 
/// </summary>
internal static class IModelExtensions
{
    /// <summary>
    /// Do a passive exchange declaration.
    /// Or
    /// (Spec method) Declare an exchange.
    ///  This method performs a "passive declare" on an exchange, which verifies whether. It will do nothing if the exchange already exists and result in a channel-levelprotocol exception (channel closure) if not.
    /// </summary>
    /// <param name="persistentConnection"></param>
    /// <param name="exchange"></param>
    /// <param name="type"></param>
    /// <param name="durable"></param>
    /// <param name="autoDelete"></param>
    /// <param name="arguments"></param>
    public static IModel ExchangeDeclare(this IRabbitMQPersistentConnection persistentConnection, string exchange, string type = ExchangeType.Topic, bool durable = true, bool autoDelete = false, IDictionary<string, object> arguments = null)
    {
        IModel channel;
        try
        {
            channel = persistentConnection.CreateModel();
            channel.ExchangeDeclarePassive(exchange);
        }
        catch
        {
            channel = persistentConnection.CreateModel();
            channel.ExchangeDeclare(exchange, type, durable, autoDelete, arguments);
        }
        return channel;
    }
    public static IModel QueueDeclareBind(this IRabbitMQPersistentConnection persistentConnection, string exchange, string queue, string routingKey)
    {
        IModel channel;
        try
        {
            channel = persistentConnection.ExchangeDeclare(exchange: exchange);
            channel.QueueDeclarePassive(queue);
        }
        catch
        {
            channel = persistentConnection.ExchangeDeclare(exchange: exchange);
            channel.QueueDeclare(queue: queue,
                            durable: true,
                            exclusive: false,
                            autoDelete: false);
        }
        channel.QueueBind(queue, exchange, routingKey, null);
        return channel;
    }
    public static bool BasicPublish(this IModel channel, IBasicProperties properties, string exchange, string routingKey, string messageBody)
    {
        channel.ConfirmSelect();
        channel.BasicPublish(
                   exchange: exchange,
                   routingKey: routingKey,
                   mandatory: true,
                   basicProperties: properties,
                   body: messageBody?.GetBytes()
                       );
        return channel.WaitForConfirms();
    }
}