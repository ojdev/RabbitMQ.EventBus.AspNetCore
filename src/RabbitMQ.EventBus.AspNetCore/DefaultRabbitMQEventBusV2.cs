using Microsoft.Extensions.Logging;

namespace RabbitMQ.EventBus.AspNetCore;

internal class DefaultRabbitMQEventBusV2 : IRabbitMQEventBus
{
    #region singleton
    private static DefaultRabbitMQEventBusV2 Instance;
    private static readonly object singleton_Lock = new();
    public static DefaultRabbitMQEventBusV2 CreateInstance(IRabbitMQPersistentConnection persistentConnection, IServiceProvider serviceProvider, ILogger<DefaultRabbitMQEventBusV2> logger)
    {
        lock (singleton_Lock) // 保证任意时刻只有一个线程才能进入判断
        {
            if (Instance == null)
            {
                Instance = new(persistentConnection, serviceProvider, logger);
            }
        }
        return Instance;
    }
    #endregion
    private readonly IRabbitMQPersistentConnection _persistentConnection;
    private readonly ILogger<DefaultRabbitMQEventBusV2> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper = new();
    private readonly Dictionary<string, IModel> subscribes;
    private readonly string GlaobalExchangeName;
    private DefaultRabbitMQEventBusV2(IRabbitMQPersistentConnection persistentConnection, IServiceProvider serviceProvider, ILogger<DefaultRabbitMQEventBusV2> logger)
    {
        _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        subscribes = new Dictionary<string, IModel>();
        GlaobalExchangeName = _persistentConnection.Configuration.ClientProvidedName;
        _logger.LogInformation("消息队列准备就绪");
    }
    #region Publish
    public async Task PublishAsync<TMessage>(TMessage message, string exchange, string routingKey, byte deliveryMode = 2, string type = "topic", CancellationToken cancellationToken = default)
    {
        var channel = _persistentConnection.ExchangeDeclare(exchange, type: type);
        channel.ModelShutdown += (object sender, ShutdownEventArgs e) =>
        {
            _logger.LogDebug($"channel shotdown.\t{e.ReplyText}");
        };
        IBasicProperties properties = channel.CreateBasicProperties();
        properties.DeliveryMode = deliveryMode; // persistent
        await RetryPolicyPublish(channel, properties, exchange, routingKey, message?.Serialize(), cancellationToken);
    }

    public Task<TResponse> PublishAsync<TMessage, TResponse>(TMessage message, string exchange, string routingKey, byte deliveryMode = 2, string type = "topic", Action errorHandler = null, CancellationToken cancellationToken = default) where TMessage : class
    => PublishAsync<TResponse>(message, exchange, routingKey, deliveryMode, type, errorHandler, cancellationToken);

    public async Task<TResponse> PublishAsync<TResponse>(object message, string exchange, string routingKey, byte deliveryMode = 2, string type = ExchangeType.Topic, Action errorHandler = null, CancellationToken cancellationToken = default)
    {
        var response = await PublishAsync(message.Serialize(), exchange, routingKey, deliveryMode, type, errorHandler, cancellationToken);
        if (response.IsNullOrWhiteSpace()) return default;
        return response.Deserialize<TResponse>();
    }

    public async Task<string> PublishAsync(string message, string exchange, string routingKey, byte deliveryMode = 2, string type = "topic", Action errorHandler = null, CancellationToken cancellationToken = default)
    {
        var channel = _persistentConnection.ExchangeDeclare(exchange, type: type);
        channel.ModelShutdown += (object sender, ShutdownEventArgs e) =>
        {
            _logger.LogDebug($"channel shotdown.\t{e.ReplyText}");
        };
        var reply = ReplyWaitConfirm(channel, exchange, routingKey);
        IBasicProperties properties = channel.CreateBasicProperties();
        properties.DeliveryMode = deliveryMode; // persistent
        var correlationId = Guid.NewGuid().ToString();
        properties.CorrelationId = correlationId;
        properties.ReplyTo = reply.routingKey;
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);
        await RetryPolicyPublish(channel, properties, exchange, routingKey, message, cancellationToken);
        var result = await tcs.Task;
        _logger.LogDebug($"delete message queue {reply.queueName}");
        channel.QueueDelete(reply.queueName);
        _logger.LogDebug($"delete message queue {reply.queueName} done.");
        return result;
    }
    #endregion
    #region Old Version Publish
    public void Publish<TMessage>(TMessage message, string exchange, string routingKey, byte deliveryMode = 2, string type = "topic")
    => Publish(message?.Serialize(), exchange, routingKey, deliveryMode, type);

    public void Publish(string message, string exchange, string routingKey, byte deliveryMode = 2, string type = "topic")
    {
        var _publishChannel = _persistentConnection.ExchangeDeclare(exchange, type: type);
        IBasicProperties properties = _publishChannel.CreateBasicProperties();
        properties.DeliveryMode = deliveryMode; // persistent
        _publishChannel.BasicPublish(properties, exchange, routingKey, message);
    }
    #endregion
    #region Subscribe
    public void Subscribe(Type eventType, string type = "topic")
    {
        var attributes = eventType.GetCustomAttributes(typeof(EventBusAttribute), true);
        var millisecondsDelay = (int?)_persistentConnection?.Configuration?.ConsumerFailRetryInterval.TotalMilliseconds ?? 1000;
        foreach (var attribute in attributes)
        {
            if (attribute is EventBusAttribute attr)
            {
                string queue = attr.Queue ?? (_persistentConnection.Configuration.Prefix == QueuePrefixType.ExchangeName
                    ? $"{attr.Exchange}.{eventType.Name}"
                    : (eventType.FullName ?? $"{GlaobalExchangeName}.{eventType.Name}"));

                var onlyKey = $"{attr.Exchange}_{queue}_{attr.RoutingKey}";
                if (!subscribes.TryGetValue(onlyKey, out IModel channel))
                {
                    channel = _persistentConnection.QueueDeclareBind(attr.Exchange, queue, attr.RoutingKey);
                }
                channel.QueueBind(queue, attr.Exchange, attr.RoutingKey, null);
                channel.BasicQos(0, attr.BasicQos ?? _persistentConnection.Configuration.PrefetchCount, false);
                subscribes[onlyKey] = channel;
                EventingBasicConsumer consumer = new(channel);
                consumer.Received += async (model, ea) =>
                {
                    string body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var props = ea.BasicProperties;
                    var replyProps = channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;

                    bool isAck = false;
                    try
                    {
                        await ProcessEventAsync(body, eventType, ea);
                        //不确定是否需要改变Multiple是否需要改为true
                        channel.BasicAck(ea.DeliveryTag, multiple: false);
                        isAck = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(new EventId(ex.HResult), ex, ex.Message);
                    }
                    finally
                    {
                        _logger.LogInformation($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}\t{isAck}\t{ea.Exchange}\t{ea.RoutingKey}\t{body}");
                        if (!isAck)
                        {
                            await Task.Delay(millisecondsDelay);
                            channel.BasicNack(ea.DeliveryTag, false, true);
                        }
                    }
                };
                channel.CallbackException += (sender, ex) =>
                {
                    _logger.LogError(new EventId(ex.Exception.HResult), ex.Exception, ex.Exception.Message);
                };
                channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
            }
        }
    }
    public void Subscribe(Type eventType, Type responseType, string type = "topic")
    {
        var attributes = eventType.GetCustomAttributes(typeof(EventBusAttribute), true);
        var millisecondsDelay = (int?)_persistentConnection?.Configuration?.ConsumerFailRetryInterval.TotalMilliseconds ?? 1000;
        foreach (var attribute in attributes)
        {
            if (attribute is EventBusAttribute attr)
            {
                string queue = attr.Queue ?? (_persistentConnection.Configuration.Prefix == QueuePrefixType.ExchangeName
                    ? $"{attr.Exchange}.{eventType.Name}"
                    : (eventType.FullName ?? $"{GlaobalExchangeName}.{eventType.Name}"));

                var onlyKey = $"{attr.Exchange}_{queue}_{attr.RoutingKey}";
                _logger.LogWarning($"onlyKey => {onlyKey}");
                if (!subscribes.TryGetValue(onlyKey, out IModel channel))
                {
                    channel = _persistentConnection.QueueDeclareBind(attr.Exchange, queue, attr.RoutingKey);
                }
                channel.QueueBind(queue, attr.Exchange, attr.RoutingKey, null);
                channel.BasicQos(0, attr.BasicQos ?? _persistentConnection.Configuration.PrefetchCount, false);
                subscribes[onlyKey] = channel;
                EventingBasicConsumer consumer = new(channel);
                consumer.Shutdown += (s, e) => _logger.LogError($"{eventType}\t{responseType}\tConsumer Shutdown.\t{consumer.ShutdownReason}\t{e.ReplyText}");
                consumer.Unregistered += (s, e) => _logger.LogError($"{eventType}\t{responseType}\tConsumer Unregistered.");
                consumer.ConsumerCancelled += (s, e) => _logger.LogError($"{eventType}\t{responseType}\tConsumer ConsumerCancelled.");
                consumer.Received += async (model, ea) =>
                {
                    string body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    _logger.LogDebug($"received message: {body}");
                    var props = ea.BasicProperties;
                    var replyProps = channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;
                    bool isAck = false;
                    try
                    {
                        _logger.LogDebug($"received process event start.");
                        var response = await ProcessEventAsync(body, eventType, responseType, ea);
                        _logger.LogDebug($"received process event end.");
                        isAck = true;
                        if (!string.IsNullOrEmpty(replyProps.CorrelationId))
                        {
                            _logger.LogDebug($"reply message.{attr.Exchange} {props.ReplyTo} {response}");
                            channel.ConfirmSelect();
                            channel.BasicPublish(exchange: attr.Exchange, routingKey: props.ReplyTo, mandatory: true, basicProperties: replyProps, body: response.GetBytes());
                            var replyIsOk = channel.WaitForConfirms();
                            _logger.LogDebug($"reply message confirm {replyIsOk}.");
                        }
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        await Task.Yield();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(new EventId(ex.HResult), ex, ex.Message);
                    }
                    finally
                    {
                        _logger.LogInformation($"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}\t{isAck}\t{ea.Exchange}\t{ea.RoutingKey}\t{body}");
                        if (!isAck)
                        {
                            await Task.Delay(millisecondsDelay);
                            channel.BasicNack(ea.DeliveryTag, false, true);
                        }
                    }
                };
                channel.CallbackException += (sender, ex) =>
                {
                    _logger.LogError(new EventId(ex.Exception.HResult), ex.Exception, ex.Exception.Message);
                };
                var consumerTag = channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
                _logger.LogInformation($"consumer bind\t{consumerTag}");
            }
        }
    }
    /// <summary>
    /// 消息发布的回复通知订阅
    /// </summary>
    /// <returns></returns>
    private (string queueName, string routingKey) ReplyWaitConfirm(IModel channel, string exchange, string routingKey)
    {
        var timespanStr = $"{DateTimeOffset.Now.ToUnixTimeSeconds()}";
        var replyQueueName = $"{exchange}_{timespanStr}_ReplyQueue";
        routingKey = $"{routingKey}_{timespanStr}_reply";
        var qdOk = channel.QueueDeclare(queue: replyQueueName,
                         durable: true,
                         exclusive: false,
                         autoDelete: false);
        channel.QueueBind(replyQueueName, exchange, routingKey, null);
        var replyConsumer = new EventingBasicConsumer(channel);
        replyConsumer.Received += (model, ea) =>
        {
            if (!callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out TaskCompletionSource<string> tcs))
                return;
            var response = Encoding.UTF8.GetString(ea.Body.ToArray());
            tcs.TrySetResult(response);
        };
        var consumeResult = channel.BasicConsume(consumer: replyConsumer, queue: replyQueueName, autoAck: true);
        return (qdOk.QueueName, routingKey);
    }

    #endregion

    private async Task<string> ProcessEventAsync(string body, Type eventType, Type responseType, BasicDeliverEventArgs args)
    {
        Type eventHandlerType = typeof(IEventResponseHandler<,>).MakeGenericType(eventType, responseType);
        dynamic eventHandler = _serviceProvider.GetRequiredService(eventHandlerType);
        if (eventHandler == null)
        {
            throw new InvalidOperationException(eventHandler.GetType().Name);
        }
        Type concreteType = eventHandlerType;
        var r = (object)await concreteType.GetMethod("HandleAsync").Invoke(
               eventHandler,
               new object[] {
                Activator.CreateInstance(typeof(HandlerEventArgs<>).MakeGenericType(eventType), new object[] { body, args })
               });
        _logger.LogInformation($"ProcessEventAsync :{r?.Serialize()}");
        return r?.Serialize();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="body"></param>
    /// <param name="eventType"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    private async Task ProcessEventAsync(string body, Type eventType, BasicDeliverEventArgs args)
    {
        //Type eventHandlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
        //dynamic eventHandler = _serviceProvider.GetRequiredService(eventHandlerType);
        //if (eventHandler == null)
        //{
        //    throw new InvalidOperationException(eventHandler.GetType().Name);
        //}
        ////IEventHandler<RabbitMQ.EventBus.AspNetCore.Simple.Controllers.MessageBody>

        //object logger = _serviceProvider.GetRequiredService(typeof(ILogger<>).MakeGenericType(eventType));
        //Type concreteType = eventHandlerType.MakeGenericType(eventType);
        //await (Task)concreteType.GetMethod(nameof(IEventHandler<IEvent>.Handle)).Invoke(
        //       eventHandler,
        //       new object[] {
        //        Activator.CreateInstance(typeof(HandlerEventArgs<>).MakeGenericType(eventType), new object[] {  body, args.Redelivered, args.Exchange, args.RoutingKey, logger })
        //       });


        using var scope = _serviceProvider.CreateScope();
        foreach (Type eventHandleType in typeof(IEventHandler<>).GetMakeGenericType(eventType))
        {
            object eventHandler = scope.ServiceProvider.GetRequiredService(eventHandleType);
            object logger = scope.ServiceProvider.GetRequiredService(typeof(ILogger<>).MakeGenericType(eventType));
            if (eventHandler == null)
            {
                throw new InvalidOperationException(eventHandleType.Name);
            }
            Type concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
            await (Task)concreteType.GetMethod(nameof(IEventHandler<IEvent>.Handle)).Invoke(
                eventHandler,
                new object[] {
                         Activator.CreateInstance(typeof(EventHandlerArgs<>).MakeGenericType(eventType), new object[] { body, args.Redelivered, args.Exchange, args.RoutingKey, logger })
                });
        }
    }
    /// <summary>
    /// 指数重试机制的发消息机制
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="properties"></param>
    /// <param name="exchange"></param>
    /// <param name="routingKey"></param>
    /// <param name="messageBody"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task RetryPolicyPublish(IModel channel, IBasicProperties properties, string exchange, string routingKey, string messageBody, CancellationToken cancellationToken)
    {
        var policy = Policy
            .HandleResult<bool>(r => !r)
            .WaitAndRetryAsync(
                5,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, context) =>
                {
                    _logger.LogError($"Publish failed! retry after {timeSpan.TotalSeconds} seconds...");
                }
            );
        await policy.ExecuteAsync(async () =>
         {
             var isOk = channel.BasicPublish(properties, exchange, routingKey, messageBody);
             if (isOk)
             {
                 _logger.LogInformation($"Published successfully.\t{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}\t{exchange}\t{routingKey}\t{messageBody}");
                 if (string.IsNullOrEmpty(properties.CorrelationId))
                     cancellationToken.Register(() => callbackMapper.TryRemove(properties.CorrelationId, out var tmp));
             }
             else
                 _logger.LogError($"Publish failed!\t{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}\t{exchange}\t{routingKey}\t{messageBody}");
             return await Task.FromResult(isOk);
         });
    }

}
