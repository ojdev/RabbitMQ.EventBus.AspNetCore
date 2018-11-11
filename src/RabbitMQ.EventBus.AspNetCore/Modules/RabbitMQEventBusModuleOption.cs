using System;

namespace RabbitMQ.EventBus.AspNetCore.Modules
{
    /// <summary>
    /// 模块
    /// </summary>
    public sealed class RabbitMQEventBusModuleOption
    {
        private readonly IEventHandlerModuleFactory handlerFactory;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlerFactory"></param>
        public RabbitMQEventBusModuleOption(IEventHandlerModuleFactory handlerFactory)
        {
            this.handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
        }
        /// <summary>
        /// 添加模块
        /// </summary>
        /// <param name="module"></param>
        public void AddModule(IModuleHandle module)
        {
            handlerFactory.TryAddMoudle(module);
        }
    }
}
