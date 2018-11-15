# RabbitMQ.EventBus.AspNetCore
[![DNC](https://img.shields.io/badge/.netcore-%3E%3D2.0-green.svg)](#)
[![AppVeyor](https://img.shields.io/appveyor/ci/ojdev/rabbitmq-eventbus-aspnetcore.svg?style=popout)](https://ci.appveyor.com/project/ojdev/rabbitmq-eventbus-aspnetcore)
[![NuGet](https://img.shields.io/nuget/v/RabbitMQ.EventBus.AspNetCore.svg?style=popout)](https://www.nuget.org/packages/RabbitMQ.EventBus.AspNetCore)
[![NuGet](https://img.shields.io/nuget/dt/RabbitMQ.EventBus.AspNetCore.svg?style=popout)](https://www.nuget.org/packages/RabbitMQ.EventBus.AspNetCore)
[![GitHub license](https://img.shields.io/github/license/ojdev/RabbitMQ.EventBus.AspNetCore.svg)](https://github.com/ojdev/RabbitMQ.EventBus.AspNetCore/blob/master/LICENSE)

### 使用说明

#### 1. 注册
~~~ csharp
public void ConfigureServices(IServiceCollection services)
{
    string assemblyName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
    services.AddRabbitMQEventBus("amqp://guest:guest@192.168.0.252:5672/", eventBusOptionAction: eventBusOption =>
    {
        eventBusOption.ClientProvidedAssembly(assemblyName);
        eventBusOption.EnableRetryOnFailure(true, 5000, TimeSpan.FromSeconds(30));
        eventBusOption.RetryOnConsumeFailure(TimeSpan.FromSeconds(1));
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
~~~

## Modules
### RabbitMQ.EventBus.AspNetCore.Butterfly

[![NuGet](https://img.shields.io/nuget/v/RabbitMQ.EventBus.AspNetCore.Butterfly.svg?style=popout)](https://www.nuget.org/packages/RabbitMQ.EventBus.AspNetCore.Butterfly)
[![NuGet](https://img.shields.io/nuget/dt/RabbitMQ.EventBus.AspNetCore.Butterfly.svg?style=popout)](https://www.nuget.org/packages/RabbitMQ.EventBus.AspNetCore.Butterfly)

#### 使用方法
~~~ csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceTracer tracer)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    ···
    app.RabbitMQEventBusModule(options =>
    {
        options.AddButterfly(tracer);
    });
    app.UseMvc();
}
~~~
