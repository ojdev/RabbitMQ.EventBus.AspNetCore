using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.EventBus.AspNetCore.Attributes;
using RabbitMQ.EventBus.AspNetCore.Events;
using RabbitMQ.EventBus.AspNetCore.Factories;
using RabbitMQ.EventBus.AspNetCore.Modules;
using System;
using System.Linq;
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
        }

        private IModel _publishChannel;
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
            if (_publishChannel?.IsOpen != true)
            {
                if (_persistentConnection.IsConnected)
                {
                    _persistentConnection.TryConnect();
                }
                _publishChannel = _persistentConnection.ExchangeDeclare(exchange, type: type);
                _publishChannel.BasicReturn += async (se, ex) => await Task.Delay((int)_persistentConnection.Configuration.ConsumerFailRetryInterval.TotalMilliseconds).ContinueWith(t => Publish(body, ex.Exchange, ex.RoutingKey));
            }
            IBasicProperties properties = _publishChannel.CreateBasicProperties();
            properties.DeliveryMode = 2; // persistent
            _publishChannel.BasicPublish(exchange: exchange,
                             routingKey: routingKey,
                             mandatory: true,
                             basicProperties: properties,
                             body: body.GetBytes());
            _logger.Information(body);
            _eventHandlerFactory?.PubliushEvent(new EventBusArgs(_persistentConnection.Endpoint, exchange, "", routingKey, type, _persistentConnection.Configuration.ClientProvidedName, body, true));
        }
        public void Subscribe<TEvent, THandler>(string type = ExchangeType.Topic)
            where TEvent : IEvent
            where THandler : IEventHandler<TEvent>
        {
            Subscribe(typeof(TEvent), typeof(THandler));
            #region MyRegion
            /*object attribute = typeof(TEvent).GetCustomAttributes(typeof(EventBusAttribute), true).FirstOrDefault();
                if (attribute is EventBusAttribute attr)
                {
                    string queue = attr.Queue ?? $"{ attr.Exchange }.{ typeof(TEvent).Name }";
                    if (!_persistentConnection.IsConnected)
                    {
                        _persistentConnection.TryConnect();
                    }
                    IModel channel;
                    #region snippet
                    try
                    {
                        channel = _persistentConnection.ExchangeDeclare(exchange: attr.Exchange, type: type);
                        channel.QueueDeclarePassive(queue);
                    }
                    catch
                    {
                        channel = _persistentConnection.ExchangeDeclare(exchange: attr.Exchange, type: type);
                        channel.QueueDeclare(queue: queue,
                                             durable: true,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: null);
                    }
                    #endregion
                    channel.QueueBind(queue, attr.Exchange, attr.RoutingKey, null);
                    channel.BasicQos(0, 1, false);
                    EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                    consumer.Received += async (model, ea) =>
                    {
                        string body = Encoding.UTF8.GetString(ea.Body);
                        bool isAck = false;
                        try
                        {
                            await ProcessEvent<TEvent, THandler>(body);
                            channel.BasicAck(ea.DeliveryTag, multiple: false);
                            isAck = true;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(new EventId(ex.HResult), ex, ex.Message);
                        }
                        finally
                        {
                            _logger.Information($"RabbitMQEventBus\t{DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss")}\t{isAck}\t{ea.Exchange}\t{ea.RoutingKey}\t{body}");
                        }
                    };
                    channel.CallbackException += (sender, ex) =>
                    {

                    };
                    channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
                }*/
            #endregion
        }

        public void Subscribe(Type eventType, Type eventHandleType, string type = ExchangeType.Topic)
        {
            object attribute = eventType.GetCustomAttributes(typeof(EventBusAttribute), true).FirstOrDefault();
            if (attribute is EventBusAttribute attr)
            {
                string queue = attr.Queue ?? $"{ attr.Exchange }.{ eventType.Name }";
                if (!_persistentConnection.IsConnected)
                {
                    _persistentConnection.TryConnect();
                }
                IModel channel;
                #region snippet
                try
                {
                    channel = _persistentConnection.ExchangeDeclare(exchange: attr.Exchange, type: type);
                    channel.QueueDeclarePassive(queue);
                }
                catch
                {
                    channel = _persistentConnection.ExchangeDeclare(exchange: attr.Exchange, type: type);
                    channel.QueueDeclare(queue: queue,
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);
                }
                #endregion
                channel.QueueBind(queue, attr.Exchange, attr.RoutingKey, null);
                //channel.BasicQos(0, 1, false);
                EventingBasicConsumer consumer = new EventingBasicConsumer(channel);
                consumer.Received += async (model, ea) =>
                {
                    string body = Encoding.UTF8.GetString(ea.Body);
                    bool isAck = false;
                    try
                    {
                        await ProcessEvent(body, eventType, eventHandleType);
                        channel.BasicAck(ea.DeliveryTag, multiple: false);
                        isAck = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(new EventId(ex.HResult), ex, ex.Message);
                        //(int)_persistentConnection.Configuration.ConsumerFailRetryInterval.TotalMilliseconds
                        await Task.Delay(200).ContinueWith(p => channel.BasicNack(ea.DeliveryTag, false, true));
                        channel.BasicNack(ea.DeliveryTag, false, true);
                    }
                    finally
                    {
                        _eventHandlerFactory?.SubscribeEvent(new EventBusArgs(_persistentConnection.Endpoint, ea.Exchange, queue, attr.RoutingKey, type, _persistentConnection.Configuration.ClientProvidedName, body, isAck));
                        _logger.Information($"RabbitMQEventBus\t{DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss")}\t{isAck}\t{ea.Exchange}\t{ea.RoutingKey}\t{body}");
                    }
                };
                channel.CallbackException += (sender, ex) =>
                {
                    _logger.LogError(new EventId(ex.Exception.HResult), ex.Exception, ex.Exception.Message);
                };
                channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
            }
        }
        private async Task ProcessEvent<TEvent, TEventHandle>(string body)
            where TEvent : IEvent
            where TEventHandle : IEventHandler<TEvent>
        {
            TEventHandle eventHandler = _serviceProvider.GetRequiredService<TEventHandle>();
            TEvent integrationEvent = JsonConvert.DeserializeObject<TEvent>(body);
            await eventHandler.Handle(integrationEvent);
        }
        private async Task ProcessEvent(string body, Type eventType, Type eventHandleType)
        {
            object eventHandler = _serviceProvider.GetRequiredService(eventHandleType);
            if (eventHandler == null)
            {
                throw new InvalidOperationException(eventHandleType.Name);
            }
            object integrationEvent = JsonConvert.DeserializeObject(body, eventType);
            Type concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
            await (Task)concreteType.GetMethod(nameof(IEventHandler<IEvent>.Handle)).Invoke(eventHandler, new object[] { integrationEvent });
        }
    }
}
