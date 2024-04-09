namespace BlazorEventBus.Abstractions;

/// <summary>
/// Interface definition for event handler registered in dependency injection container 
/// </summary>
/// <typeparam name="TEvent">Event type</typeparam>
public interface IEventHandler<in TEvent> where TEvent : class, IEvent
{
    Task HandleAsync(TEvent @event, CancellationToken token = default);
}