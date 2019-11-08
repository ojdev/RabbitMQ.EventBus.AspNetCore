using Microsoft.Extensions.Logging;
using RabbitMQ.EventBus.AspNetCore.Events;
using System;
using System.Threading.Tasks;

namespace RabbitMQ.EventBus.AspNetCore.Simple.Controllers
{
    public class MessageBodyHandle111 : IEventHandler<MessageBody1>, IDisposable
    {
        private Guid id;
        private readonly ILogger<MessageBodyHandle> _logger;

        public MessageBodyHandle111(ILogger<MessageBodyHandle> logger)
        {
            id = Guid.NewGuid();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public void Dispose()
        {
            Console.WriteLine("释放");
        }

        public Task Handle(EventHandlerArgs<MessageBody1> args)
        {
            Console.WriteLine("==================================================");
            Console.WriteLine(id + "=>" + typeof(MessageBody1).Name);
            Console.WriteLine(args.Event.Body);
            Console.WriteLine(args.Original);
            Console.WriteLine(args.Redelivered);
            Console.WriteLine("==================================================");
            return Task.CompletedTask;
        }
    }
}
