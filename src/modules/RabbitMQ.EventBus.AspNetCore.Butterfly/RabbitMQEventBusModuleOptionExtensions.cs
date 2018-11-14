using Housecool.Butterfly.Client.Tracing;
using RabbitMQ.EventBus.AspNetCore.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RabbitMQEventBusModuleOptionExtensions
    {
        /// <summary>
        /// Butterfly已停止维护
        /// </summary>
        /// <param name="build"></param>
        /// <param name="tracer"></param>
        /// <returns></returns>
        public static RabbitMQEventBusModuleOption AddButterfly(this RabbitMQEventBusModuleOption build, IServiceTracer tracer)
        {
            build.AddModule(ButterflyModuleHandler.Handle(tracer));
            return build;
        }
    }
}
