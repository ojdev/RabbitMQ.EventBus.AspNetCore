﻿namespace RabbitMQ.EventBus.AspNetCore.Events;
/// <summary>
/// EventBus消息处理
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public interface IEventHandler<TEvent> where TEvent : IEvent
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    Task Handle(EventHandlerArgs<TEvent> args);
}
