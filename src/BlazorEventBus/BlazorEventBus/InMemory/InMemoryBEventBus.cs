using BlazorEventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorEventBus.InMemory;

public sealed class InMemoryBEventBus : IBEventBus
{
    public async Task PublishAsync(IEnumerable<IEvent> events, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public async Task PublishAsync(IEvent @event, CancellationToken token = default)
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