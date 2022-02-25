using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.EventBus.AspNetCore.Attributes;
using RabbitMQ.EventBus.AspNetCore.Configurations;
using RabbitMQ.EventBus.AspNetCore.Events;
using RabbitMQ.EventBus.AspNetCore.Factories;
using RabbitMQ.EventBus.AspNetCore.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.EventBus.AspNetCore
{
    /// <summary>
    /// 
    /// </summary>
    internal class DefaultRabbitMQEventBus : IRabbitMQEventBus
    {
        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly ILogger<DefaultRabbitMQEventBus> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventHandlerModuleFactory _eventHandlerFactory;
        private readonly Dictionary<string, IModel> subscribes;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="persistentConnection"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="eventHandlerFactory"></param>
        /// <param name="logger"></param>
        public DefaultRabbitMQEventBus(IRabbitMQPersistentConnection persistentConnection, IServiceProvider serviceProvider, IEventHandlerModuleFactory eventHandlerFactory, ILogger<DefaultRabbitMQEventBus> logger)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _eventHandlerFactory = eventHandlerFactory ?? throw new ArgumentNullException(nameof(eventHandlerFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            subscribes = new Dictionary<string, IModel>();
            _logger.LogInformation("消息队列准备就绪");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="message"></param>
        /// <param name="exchange"></param>
        /// <param name="routingKey"></param>
        /// <param name="type"></param>
        public void Publish<TMessage>(TMessage message, string exchange, string routingKey, string type = ExchangeType.Topic)
        {
            string body = message.Serialize();
            using var _publishChannel = _persistentConnection.ExchangeDeclare(exchange, type: type);
            _publishChannel.BasicReturn += async (se, ex) => await Task.Delay((int)_persistentConnection.Configuration.ConsumerFailRetryInterval.TotalMilliseconds).ContinueWith(t => Publish(body, ex.Exchange, ex.RoutingKey));
            IBasicProperties properties = _publishChannel.CreateBasicProperties();
            properties.DeliveryMode = 2; // persistent
            _publishChannel.BasicPublish(exchange: exchange,
                             routingKey: routingKey,
                             mandatory: true,
                             basicProperties: properties,
                             body: body.GetBytes());
            _logger.WriteLog(_persistentConnection.Configuration.Level, $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}\t{exchange}\t{routingKey}\t{body}");
            _eventHandlerFactory?.PubliushEvent(new EventBusArgs(_persistentConnection.Endpoint, exchange, "", routingKey, type, _persistentConnection.Configuration.ClientProvidedName, body, true));
        }
        public void Subscribe(Type eventType, string type = ExchangeType.Topic)
        {
            var attributes = eventType.GetCustomAttributes(typeof(EventBusAttribute), true);
            var millisecondsDelay = (int?)_persistentConnection?.Configuration?.ConsumerFailRetryInterval.TotalMilliseconds ?? 1000;
            foreach (var attribute in attributes)
            {
                if (attribute is EventBusAttribute attr)
                {
                    string queue = attr.Queue ?? (_persistentConnection.Configuration.Prefix == QueuePrefixType.ExchangeName
                        ? $"{ attr.Exchange }.{ eventType.Name }"
                        : $"{_persistentConnection.Configuration.ClientProvidedName}.{ eventType.Name }");

                    var onlyKey = $"{attr.Exchange}_{queue}_{attr.RoutingKey}";
                    subscribes.TryGetValue(onlyKey, out IModel channel);
                    #region snippet
                    var arguments = new Dictionary<string, object>();

                    #region 死信队列设置
                    if (_persistentConnection.Configuration.DeadLetterExchange.Enabled)
                    {
                        string deadExchangeName = $"{_persistentConnection.Configuration.DeadLetterExchange.ExchangeNamePrefix}{_persistentConnection.Configuration.DeadLetterExchange.CustomizeExchangeName ?? attr.Exchange}{_persistentConnection.Configuration.DeadLetterExchange.ExchangeNameSuffix}";
                        string deadQueueName = $"{_persistentConnection.Configuration.DeadLetterExchange.ExchangeNamePrefix}{queue}{_persistentConnection.Configuration.DeadLetterExchange.ExchangeNameSuffix}";
                        IModel dlxChannel;
                        try
                        {
                            dlxChannel = _persistentConnection.ExchangeDeclare(exchange: deadExchangeName, type: type);
                            dlxChannel.QueueDeclarePassive(deadQueueName);
                        }
                        catch
                        {
                            dlxChannel = _persistentConnection.ExchangeDeclare(exchange: deadExchangeName, type: type);
                            dlxChannel.QueueDeclare(queue: deadQueueName,
                                                durable: true,
                                                exclusive: false,
                                                autoDelete: false,
                                                arguments: null);
                        }
                        dlxChannel.QueueBind(deadQueueName, deadExchangeName, attr.RoutingKey, null);
                        arguments.Add("x-dead-letter-exchange", deadExchangeName);
                    }
                    #endregion

                    try
                    {
                        channel = _persistentConnection.ExchangeDeclare(exchange: attr.Exchange, type: type);
                        channel.QueueDeclarePassive(queue);
                    }
                    catch
                    {
                        channel = _persistentConnection.ExchangeDeclare(exchange: attr.Exchange, type: type);
                        if (_persistentConnection.Configuration.MessageTTL != null && _persistentConnection.Configuration.MessageTTL > 0)
                        {
                            arguments.Add("x-message-ttl", _persistentConnection.Configuration.MessageTTL);
                        }
                        channel.QueueDeclare(queue: queue,
                                             durable: true,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: arguments);
                    }
                    #endregion
                    channel.QueueBind(queue, attr.Exchange, attr.RoutingKey, null);
                    channel.BasicQos(0, _persistentConnection.Configuration.PrefetchCount, false);
                    subscribes[onlyKey] = channel;
                    EventingBasicConsumer consumer = new(channel);
                    consumer.Received += async (model, ea) =>
                    {
                        string body = Encoding.UTF8.GetString(ea.Body.ToArray());
                        bool isAck = false;
                        try
                        {
                            await ProcessEvent(body, eventType, ea);
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
                            _eventHandlerFactory?.SubscribeEvent(new EventBusArgs(_persistentConnection.Endpoint, ea.Exchange, queue, attr.RoutingKey, type, _persistentConnection.Configuration.ClientProvidedName, body, isAck));
                            _logger.WriteLog(_persistentConnection.Configuration.Level, $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}\t{isAck}\t{ea.Exchange}\t{ea.RoutingKey}\t{body}");
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="body"></param>
        /// <param name="eventType"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task ProcessEvent(string body, Type eventType, BasicDeliverEventArgs args)
        {
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
    }
}
