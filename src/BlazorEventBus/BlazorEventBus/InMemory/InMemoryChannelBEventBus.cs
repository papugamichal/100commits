using System.Threading.Channels;
using BlazorEventBus.Abstractions;
using JetBrains.Annotations;

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

    [MustDisposeResource]
    public IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class, IEvent
    {
        return new Subscription<TEvent>(this, handler);
    }
    
    [MustDisposeResource]
    private sealed class Subscription<TEvent> : IDisposable where TEvent : IEvent
    {
        private readonly Func<TEvent, Task> _handler;
        private Func<TEvent, Task> handler;
        
        public Subscription(InMemoryChannelBEventBus clazz, Func<TEvent, Task> handler)
        {
            _handler = handler;
            
            var channel = Channel.CreateBounded<TEvent>(1);
            var t = Task.Run(async () =>
            {
                await foreach (var @event in channel.Reader.ReadAllAsync(CancellationToken.None))
                {
                    await handler(@event);
                };
            });

            clazz.RegisterSubscription<TEvent>(this);

        }
        private void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
        }

        [HandlesResourceDisposal]
        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~Subscription()
        {
            ReleaseUnmanagedResources();
        }
    }

    private void RegisterSubscription<TEvent>(Subscription<TEvent> subscription) where TEvent : IEvent
    {
        throw new NotImplementedException();
    }
}