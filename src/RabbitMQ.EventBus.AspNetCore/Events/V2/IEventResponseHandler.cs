namespace RabbitMQ.EventBus.AspNetCore.Events;

public interface IEventResponseHandler<TEvent, TResponse> where TEvent : IEvent
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    Task<TResponse> HandleAsync(HandlerEventArgs<TEvent> args);
}