using Microsoft.Extensions.Logging;
using RabbitMQ.EventBus.AspNetCore.Attributes;
using RabbitMQ.EventBus.AspNetCore.Events;
using System;
using System.Threading.Tasks;

namespace Simple.WebApi.Controllers
{
    [EventBus(Exchange = "RabbitMQ.EventBus.Simple", RoutingKey = "rabbitmq.eventbus.test")]
    public class MessageBody : IEvent
    {
        public string Body { get; set; }
        public DateTimeOffset Time { get; set; }
    }
    public class MessageBodyHandle : IEventHandler<MessageBody>
    {
        private readonly ILogger<MessageBodyHandle> _logger;

        public MessageBodyHandle(ILogger<MessageBodyHandle> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task Handle(MessageBody message)
        {
            _logger.Information(message.Serialize());
            return Task.CompletedTask;
        }
    }
}
