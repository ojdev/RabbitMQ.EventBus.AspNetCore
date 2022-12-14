# [RabbitMQ.EventBus.AspNetCore](https://github.com/ojdev/RabbitMQ.EventBus.AspNetCore)

该包为一个基于官方RabbitMQ.Client的二次封装包，专门针对Asp.Net Core项目进行开发，在微服务中进行消息的传递使用起来比较方便。

目前功能：

- [x] 发布/订阅
- [x] 死信队列
- [x] RPC功能（实验性）

### 使用说明(>=6.0.0)

#### 1. 注册
~~~ csharp
public void ConfigureServices(IServiceCollection services)
{
    string assemblyName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
    services.AddRabbitMQEventBus("localhost", 5672, "guest", "guest", "", eventBusOptionAction: eventBusOption =>
    {
        eventBusOption.ClientProvidedAssembly(assemblyName);
        eventBusOption.EnableRetryOnFailure(true, 5000, TimeSpan.FromSeconds(30));
        eventBusOption.RetryOnFailure(TimeSpan.FromSeconds(1));
        eventBusOption.MessageTTL(2000);
        eventBusOption.SetBasicQos(10);
        eventBusOption.DeadLetterExchangeConfig(config =>
        {
            config.Enabled = false;
            config.ExchangeNameSuffix = "-test";
        });
    });

    //or
    //
    //services.AddRabbitMQEventBus(() => "amqp://guest:guest@localhost:5672/", eventBusOptionAction: eventBusOption =>
    //{
    //    eventBusOption.ClientProvidedAssembly(assemblyName);
    //    eventBusOption.EnableRetryOnFailure(true, 5000, TimeSpan.FromSeconds(30));
    //    eventBusOption.RetryOnFailure(TimeSpan.FromSeconds(1));
    //    eventBusOption.MessageTTL(2000);
    //    eventBusOption.SetBasicQos(10);
    //    eventBusOption.DeadLetterExchangeConfig(config =>
    //    {
    //        config.Enabled = false;
    //        config.ExchangeNameSuffix = "-test";
    //    });
    //});
}
~~~
#### 2. 发消息
##### 2.1 直接发送消息
~~~ csharp
[Route("api/[controller]")]
[ApiController]
public class EventBusController : ControllerBase
{
    private readonly IRabbitMQEventBus _eventBus;

    public EventBusController(IRabbitMQEventBus eventBus)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    // GET api/values
    [HttpGet]
    public IActionResult Send()
    {
        _eventBus.Publish(new
        {
            Body = "发送消息",
            Time = DateTimeOffset.Now
        }, exchange: "RabbitMQ.EventBus.Simple", routingKey: "rabbitmq.eventbus.test");
        return Ok();
    }
}
~~~
##### 2.1 发送消息并等待回复
~~~ csharp
[Route("api/[controller]")]
[ApiController]
public class EventBusController : ControllerBase
{
    private readonly IRabbitMQEventBus _eventBus;

    public EventBusController(IRabbitMQEventBus eventBus)
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
                Body = $"rabbitmq.eventbus.test=>发送消息\t{1}",
                Time = DateTimeOffset.Now,
            };
            var r = await _eventBus.PublishAsync<string>(body, exchange: "RabbitMQ.EventBus.Simple", routingKey: "rabbitmq.eventbus.test");
            Console.WriteLine($"返回了{r}");
            await Task.Delay(500);
            return r;
    }
}
~~~
#### 3. 订阅消息
##### 1. 订阅消息（无回复）
~~~ csharp
[EventBus(Exchange = "RabbitMQ.EventBus.Simple", RoutingKey = "rabbitmq.eventbus.test")]
[EventBus(Exchange = "RabbitMQ.EventBus.Simple", RoutingKey = "rabbitmq.eventbus.test1")]
public class MessageBody : IEvent
{
    public string Body { get; set; }
    public DateTimeOffset Time { get; set; }
}
public class MessageBodyHandle : IEventHandler<MessageBody>, IDisposable
{
    private readonly ILogger<MessageBodyHandle> _logger;

    public MessageBodyHandle(ILogger<MessageBodyHandle> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Dispose()
    {
        Console.WriteLine("释放");
    }

    public Task Handle(EventHandlerArgs<MessageBody1> args)
    {
        _logger.Information(args.Original);
        _logger.Information(args.Redelivered);
        _logger.Information(args.Exchange);
        _logger.Information(args.RoutingKey);

        _logger.Information(args.Event.Body);
        return Task.CompletedTask;
    }
}
~~~
##### 1. 订阅消息并回复
~~~ csharp
[EventBus(Exchange = "RabbitMQ.EventBus.Simple", RoutingKey = "rabbitmq.eventbus.test")]
[EventBus(Exchange = "RabbitMQ.EventBus.Simple", RoutingKey = "rabbitmq.eventbus.test1")]
public class MessageBody : IEvent
{
    public string Body { get; set; }
    public DateTimeOffset Time { get; set; }
}
public class MessageBodyHandle : IEventResponseHandler<MessageBody, string>, IDisposable
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
        _logger.LogInformation("MessageBodyHandle Disposable.");
    }


    public Task<string> HandleAsync(HandlerEventArgs<MessageBody> args)
    {
        return Task.FromResult("收到消息，已确认" + DateTimeOffset.Now);
    }
}
~~~

### 使用说明(<=5.1.1)

#### 1. 注册
~~~ csharp
public void ConfigureServices(IServiceCollection services)
{
    string assemblyName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
    services.AddRabbitMQEventBus(()=>"amqp://guest:guest@192.168.0.252:5672/", eventBusOptionAction: eventBusOption =>
    {
        eventBusOption.ClientProvidedAssembly(assemblyName);
        eventBusOption.EnableRetryOnFailure(true, 5000, TimeSpan.FromSeconds(30));
        eventBusOption.RetryOnFailure(TimeSpan.FromMilliseconds(100));
        eventBusOption.AddLogging(LogLevel.Warning);
        eventBusOption.MessageTTL(2000);
        eventBusOption.DeadLetterExchangeConfig(config =>
        {
            config.Enabled = true;
            config.ExchangeNameSuffix = "-test";
        });
    });
    services.AddButterfly(butterfly =>
    {
        butterfly.CollectorUrl = "http://192.168.0.252:6401";
        butterfly.Service = "RabbitMQEventBusTest";
    });
}
~~~
#### 2. 订阅消息
##### 2.1 自动订阅消息
~~~ csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceTracer tracer)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    app.RabbitMQEventBusAutoSubscribe();
    app.UseMvc();
}
~~~
##### 2.2 手动订阅消息
~~~ csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env, IRabbitMQEventBus eventBus)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    eventBus.Serialize<EventMessage, EventMessageHandler>();
    app.UseMvc();
}
~~~
#### 3. 发消息
~~~ csharp
[Route("api/[controller]")]
[ApiController]
public class EventBusController : ControllerBase
{
    private readonly IRabbitMQEventBus _eventBus;

    public EventBusController(IRabbitMQEventBus eventBus)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
    }

    // GET api/values
    [HttpGet]
    public IActionResult Send()
    {
        _eventBus.Publish(new
        {
            Body = "发送消息",
            Time = DateTimeOffset.Now
        }, exchange: "RabbitMQ.EventBus.Simple", routingKey: "rabbitmq.eventbus.test");
        return Ok();
    }
}
~~~
#### 4. 订阅消息
~~~ csharp
[EventBus(Exchange = "RabbitMQ.EventBus.Simple", RoutingKey = "rabbitmq.eventbus.test")]
[EventBus(Exchange = "RabbitMQ.EventBus.Simple", RoutingKey = "rabbitmq.eventbus.test1")]
[EventBus(Exchange = "RabbitMQ.EventBus.Simple", RoutingKey = "rabbitmq.eventbus.test2")]
public class MessageBody : IEvent
{
    public string Body { get; set; }
    public DateTimeOffset Time { get; set; }
}
public class MessageBodyHandle : IEventHandler<MessageBody>, IDisposable
{
    private readonly ILogger<MessageBodyHandle> _logger;

    public MessageBodyHandle(ILogger<MessageBodyHandle> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Dispose()
    {
        Console.WriteLine("释放");
    }

    public Task Handle(EventHandlerArgs<MessageBody1> args)
    {
        _logger.Information(args.Original);
        _logger.Information(args.Redelivered);
        _logger.Information(args.Exchange);
        _logger.Information(args.RoutingKey);

        _logger.Information(args.Event.Body);
        return Task.CompletedTask;
    }
}
~~~