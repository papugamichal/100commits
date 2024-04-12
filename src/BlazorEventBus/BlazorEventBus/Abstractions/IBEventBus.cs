namespace BlazorEventBus.Abstractions;

/// <summary>
/// Interface definition for event bus functionality
/// </summary>
public interface IBEventBus
{
    Task PublishAsync(params IEvent[] @event);
    IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class, IEvent;
    IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class, IEvent;
}