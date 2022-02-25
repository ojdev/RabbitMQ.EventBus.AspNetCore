using Microsoft.AspNetCore.Mvc;
using System;

namespace RabbitMQ.EventBus.AspNetCore.Simple.Controllers
{
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
            for (int i = 0; i < 100; i++)
            {
                _eventBus.Publish(new
                {
                    Body = "rabbitmq.eventbus.test1=>发送消息",
                    Time = DateTimeOffset.Now,
                }, exchange: "RabbitMQ.EventBus.Simple", routingKey: "rabbitmq.eventbus.test1");
            }
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
