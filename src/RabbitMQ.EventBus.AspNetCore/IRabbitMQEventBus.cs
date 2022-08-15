namespace RabbitMQ.EventBus.AspNetCore;
/// <summary>
/// RabbitMQEventBus
/// </summary>
public interface IRabbitMQEventBus
{
    /// <summary>
    /// 发消息
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="message">消息体</param>
    /// <param name="exchange">交换机</param>
    /// <param name="routingKey">路由</param>
    /// <param name="deliveryMode">1不持久化,2持久化</param>
    /// <param name="type">消息类型</param>
    /// <returns></returns>
    void Publish<TMessage>(TMessage message, string exchange, string routingKey, byte deliveryMode = 2, string type = ExchangeType.Topic);
    /// <summary>
    /// 发消息
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="message">消息体</param>
    /// <param name="exchange">交换机</param>
    /// <param name="routingKey">路由</param>
    /// <param name="deliveryMode">1不持久化,2持久化</param>
    /// <param name="type">消息类型</param>
    /// <returns></returns>
    void Publish(string message, string exchange, string routingKey, byte deliveryMode = 2, string type = ExchangeType.Topic);

    /// <summary>
    /// 发消息
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="message">消息体</param>
    /// <param name="exchange">交换机</param>
    /// <param name="routingKey">路由</param>
    /// <param name="deliveryMode">1不持久化,2持久化</param>
    /// <param name="type">消息类型</param>
    /// <returns></returns>
    Task PublishAsync<TMessage>(TMessage message, string exchange, string routingKey, byte deliveryMode = 2, string type = ExchangeType.Topic, CancellationToken cancellationToken = default);
    /// <summary>
    /// 发消息
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <param name="message">消息体</param>
    /// <param name="exchange">交换机</param>
    /// <param name="routingKey">路由</param>
    /// <param name="deliveryMode">1不持久化,2持久化</param>
    /// <param name="type">消息类型</param>
    /// <returns></returns>
    //Task PublishAsync(string message, string exchange, string routingKey, byte deliveryMode = 2, string type = ExchangeType.Topic, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发消息
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="message">消息体</param>
    /// <param name="exchange">交换机</param>
    /// <param name="routingKey">路由</param>
    /// <param name="deliveryMode">1不持久化,2持久化</param>
    /// <param name="type">消息类型</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TResponse> PublishAsync<TMessage, TResponse>(TMessage message, string exchange, string routingKey, byte deliveryMode = 2, string type = ExchangeType.Topic, Action errorHandler = null, CancellationToken cancellationToken = default) where TMessage : class;
    Task<TResponse> PublishAsync<TResponse>(object message, string exchange, string routingKey, byte deliveryMode = 2, string type = ExchangeType.Topic, Action errorHandler = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 发消息
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="message">消息体</param>
    /// <param name="exchange">交换机</param>
    /// <param name="routingKey">路由</param>
    /// <param name="deliveryMode">1不持久化,2持久化</param>
    /// <param name="type">消息类型</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> PublishAsync(string message, string exchange, string routingKey, byte deliveryMode = 2, string type = ExchangeType.Topic, Action errorHandler = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// 订阅消息
    /// </summary>
    /// <param name="eventType">消息体</param>
    /// <param name="type">消息类型</param>
    void Subscribe(Type eventType, string type = ExchangeType.Topic);
    /// <summary>
    /// 订阅消息
    /// </summary>
    /// <param name="eventType">消息体</param>
    /// <param name="responseType">返回体</param>
    /// <param name="type">消息类型</param>
    void Subscribe(Type eventType, Type responseType, string type = ExchangeType.Topic);
}