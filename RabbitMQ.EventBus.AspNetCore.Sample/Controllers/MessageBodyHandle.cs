using Microsoft.Extensions.Logging;
using RabbitMQ.EventBus.AspNetCore.Events;
using System;
using System.Threading.Tasks;

namespace RabbitMQ.EventBus.AspNetCore.Simple.Controllers
{
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


}
