﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

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
        public async Task<ActionResult<string>> Get()
        {

            Console.WriteLine($"发送消息{1}");
            var body = new
            {
                requestId = Guid.NewGuid(),
                Body = $"rabbitmq.eventbus.test=>发送消息/t{1}",
                Time = DateTimeOffset.Now,
            };
            var r = await _eventBus.PublishAsync<string>(body, exchange: "RabbitMQ.EventBus.Simple", routingKey: "rabbitmq.eventbus.test");
            Console.WriteLine($"返回了{r}");
            await Task.Delay(500);
            return r;
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
