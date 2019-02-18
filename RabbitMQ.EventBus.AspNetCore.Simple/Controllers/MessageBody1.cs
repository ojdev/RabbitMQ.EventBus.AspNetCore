using RabbitMQ.EventBus.AspNetCore.Attributes;
using RabbitMQ.EventBus.AspNetCore.Events;
using System;

namespace RabbitMQ.EventBus.AspNetCore.Simple.Controllers
{
    //[EventBus(Exchange = "RabbitMQ.EventBus.Simple", RoutingKey = "rabbitmq.eventbus.test")]
    [EventBus(Exchange = "RabbitMQ.EventBus.Simple", RoutingKey = "rabbitmq.eventbus.test1")]
    public class MessageBody1 : IEvent
    {
        public string Body { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}
