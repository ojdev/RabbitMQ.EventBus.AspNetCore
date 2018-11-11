using Housecool.Butterfly.Client.Tracing;
using Housecool.Butterfly.OpenTracing;
using RabbitMQ.EventBus.AspNetCore.Modules;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Butterfly处理
    /// </summary>
    public class ButterflyModuleHandler : IModuleHandle
    {
        private static ButterflyModuleHandler _butterflyHandler;
        private readonly IServiceTracer _serviceTracer;
        public static ButterflyModuleHandler Handle(IServiceTracer serviceTracer)
        {
            if (_butterflyHandler == null)
            {
                _butterflyHandler = new ButterflyModuleHandler(serviceTracer);
            }
            return _butterflyHandler;
        }
        private ButterflyModuleHandler(IServiceTracer serviceTracer)
        {
            _serviceTracer = serviceTracer;
        }
        public async Task PublishEvent(EventBusArgs e)
        {
            if (_serviceTracer != null)
            {
                try
                {
                    await _serviceTracer.ChildTraceAsync("RabbitMQ_publish", DateTimeOffset.Now, span =>
                    {
                        span.Tags.Client().Component("RabbitMQ_Publish")
                        .Set("ClientProvidedName", e.ClientProvidedName)
                        .Set(nameof(EventBusArgs.ExchangeType), e.ExchangeType)
                        .Set(nameof(EventBusArgs.Exchange), e.Exchange)
                        .Set(nameof(EventBusArgs.RoutingKey), e.RoutingKey)
                        .Set(nameof(EventBusArgs.Message), e.Message)
                        .HttpStatusCode(e.Success ? (int)HttpStatusCode.OK : (int)HttpStatusCode.InternalServerError)
                        .PeerAddress(e.Endpoint);
                        return Task.CompletedTask;
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Butterfly故障\t" + ex.Message);
                }
            }
        }

        public async Task SubscribeEvent(EventBusArgs e)
        {
            if (_serviceTracer != null)
            {
                try
                {
                    await _serviceTracer.ChildTraceAsync("RabbitMQ_Received", DateTimeOffset.Now, span =>
                     {
                         span.Tags.Client().Component("RabbitMQ_Received")
                         .Set("ClientProvidedName", e.ClientProvidedName)
                         .Set(nameof(EventBusArgs.ExchangeType), e.ExchangeType)
                         .Set(nameof(EventBusArgs.Exchange), e.Exchange)
                         .Set(nameof(EventBusArgs.Queue), e.Queue)
                         .Set(nameof(EventBusArgs.RoutingKey), e.RoutingKey)
                         .Set(nameof(EventBusArgs.Message), e.Message)
                         .HttpStatusCode(e.Success ? (int)HttpStatusCode.OK : (int)HttpStatusCode.InternalServerError)
                         .PeerAddress(e.Endpoint);
                         return Task.CompletedTask;
                     });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Butterfly故障\t" + ex.Message);
                }
            }
        }
    }
}
