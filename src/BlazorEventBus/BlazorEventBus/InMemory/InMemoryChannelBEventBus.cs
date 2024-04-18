using BlazorEventBus.Abstractions;

namespace BlazorEventBus.InMemory;

internal class InMemoryChannelBEventBus : IBEventBus
{
    public Task PublishAsync(params IEvent[] @event)
    {
        throw new NotImplementedException();
    }

    public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class, IEvent
    {
        throw new NotImplementedException();
    }

    public IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class, IEvent
    {
        throw new NotImplementedException();
    }
}