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
        public IServiceProvider ApplicationServices;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlerFactory"></param>
        /// <param name="applicationServices"></param>
        public RabbitMQEventBusModuleOption(IEventHandlerModuleFactory handlerFactory, IServiceProvider applicationServices)
        {
            this.handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
            ApplicationServices = applicationServices;
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
