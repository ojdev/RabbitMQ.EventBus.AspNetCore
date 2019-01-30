using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.EventBus.AspNetCore.Attributes;
using RabbitMQ.EventBus.AspNetCore.Events;
using System;
using System.Threading.Tasks;

namespace RabbitMQ.EventBus.AspNetCore.Simple.Controllers
{
    [EventBus(Exchange = "RabbitMQ.EventBus.Simple", RoutingKey = "rabbitmq.eventbus.test")]
    public class MessageBody : IEvent
    {
        public string Body { get; set; }
        public DateTimeOffset Time { get; set; }
    }
    //[EventBus(Exchange = "RabbitMQ.EventBus.Simple", RoutingKey = "rabbitmq.eventbus.test")]
    [EventBus(Exchange = "RabbitMQ.EventBus.Simple", RoutingKey = "rabbitmq.eventbus.test1")]
    public class MessageBody1 : IEvent
    {
        public string Body { get; set; }
        public DateTimeOffset Time { get; set; }
    }
    public class MessageBodyHandle :  IEventHandler<MessageBody1>, IDisposable
    {
        private Guid id;
        private readonly ILogger<MessageBodyHandle> _logger;

        public MessageBodyHandle(ILogger<MessageBodyHandle> logger)
        {
            id = Guid.NewGuid();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public void Dispose()
        {
            Console.WriteLine("释放");
        }


        public Task Handle(MessageBody1 message, EventHandlerArgs args)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine(id + "=>" + typeof(MessageBody1).Name);
            Console.WriteLine(message.Serialize());
            Console.WriteLine(args.Original);
            Console.WriteLine(args.Redelivered);
            Console.WriteLine("==================================================");
            return Task.CompletedTask;
        }
    }
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IRabbitMQEventBus _eventBus;

        public ValuesController(IRabbitMQEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        // GET api/values
        [HttpGet]
        public ActionResult<string> Get()
        {
            //_eventBus.Publish(new
            //{
            //    Body = "rabbitmq.eventbus.test=>发送消息",
            //    Time = DateTimeOffset.Now
            //}, exchange: "RabbitMQ.EventBus.Simple", routingKey: "rabbitmq.eventbus.test");
            _eventBus.Publish(new
            {
                Body = "rabbitmq.eventbus.test1=>发送消息",
                Time = 432
            }, exchange: "RabbitMQ.EventBus.Simple", routingKey: "rabbitmq.eventbus.test1");
            return "Ok";
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
