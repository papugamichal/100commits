using System.Threading.Channels;
using BlazorEventBus.Abstractions;
using JetBrains.Annotations;

namespace BlazorEventBus.InMemory;

internal class InMemoryChannelBEventBus : IBEventBus
{
    public async Task PublishAsync(params IEvent[] @event)
    {
        var eventListeners = new Dictionary<Type, Channel<object>>();
        foreach (var single in @event.ToArray())
        {
            var eventType = single.GetType();
            await eventListeners[eventType].Writer.WriteAsync(single);
        }
    }

    [MustDisposeResource]
    public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class, IEvent
    {
        return new Subscription<TEvent>(this, @event =>
        {
            handler(@event);
            return Task.CompletedTask;
        });
    }

    [MustDisposeResource]
    public IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class, IEvent
    {
        return new Subscription<TEvent>(this, handler);
    }
    
    [MustDisposeResource]
    private sealed class Subscription<TEvent> : IDisposable where TEvent : IEvent
    {
        private readonly CancellationTokenSource _subscriptionLifetimeTokenSource = new();
        private readonly Task _channelReaderTask;
        private readonly InMemoryChannelBEventBus _clazz;
        
        private bool _disposed;
        
        public Subscription(InMemoryChannelBEventBus clazz, Func<TEvent, Task> handler)
        {
            var channel = Channel.CreateBounded<TEvent>(1);
            _channelReaderTask = Task.Run(async () =>
            {
                await foreach (var @event in channel.Reader.ReadAllAsync(_subscriptionLifetimeTokenSource.Token))
                {
                    await handler(@event);
                };
            });

            _clazz = clazz;
            clazz.RegisterSubscription<TEvent>(this);

        }
        private void DisposePrivate(bool dispose)
        {
            if (!dispose || _disposed)
                return;
            
            _subscriptionLifetimeTokenSource.CancelAsync()
                .ConfigureAwait(false).GetAwaiter().GetResult();
            
            _channelReaderTask?.ConfigureAwait(false)
                .GetAwaiter().GetResult();

            _clazz?.UnregisterSubscription(this);
            _disposed = true;
        }

        [HandlesResourceDisposal]
        public void Dispose()
        {
            DisposePrivate(true);
            GC.SuppressFinalize(this);
        }

        ~Subscription()
        {
            DisposePrivate(false);
        }
    }

    private void UnregisterSubscription<TEvent>(Subscription<TEvent> subscription) where TEvent : IEvent
    {
        throw new NotImplementedException();
    }

    private void RegisterSubscription<TEvent>(Subscription<TEvent> subscription) where TEvent : IEvent
    {
        
        throw new NotImplementedException();
    }
}