using BlazorEventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorEventBus.InMemory;

/// <summary>
/// Blazor event bus implementation working in-memory  
/// </summary>
internal sealed class InMemoryBEventBus : IBEventBus
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SubscriptionsRepository _subscriptionsRepository = new();

    public InMemoryBEventBus(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _serviceProvider = serviceProvider;
    }

    public async Task PublishAsync(params IEvent[] events)
    {
        var array = events.ToArray();
        if (array.Length == 0)
            return;

        Task[] tasks;
        lock (_subscriptionsRepository)
        {
            tasks = array.Select(PublishAsync).ToArray();
        }

        await Task.WhenAll(tasks);
    }

    public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class, IEvent
    {
        lock (_subscriptionsRepository)
        {
            var subscription = EventSubscription<TEvent>.Sync(this, handler);
            _subscriptionsRepository.Add(subscription);
            return subscription;
        }
    }

    public IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class, IEvent
    {
        lock (_subscriptionsRepository)
        {
            var subscription = EventSubscription<TEvent>.Async(this, handler);
            _subscriptionsRepository.Add(subscription);
            return subscription;
        }
    }

    private Task PublishAsync(IEvent? @event)
    {
        if (@event is null)
            return Task.CompletedTask;

        List<ISubscription> subscribers;
        lock (_subscriptionsRepository)
        {
            subscribers = _subscriptionsRepository.Find(subscription => subscription.IsListenerOf(@event)).ToList();
        }

        return Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var handlerType = typeof(IEventHandler<>).MakeGenericType(@event.GetType());
            var handlerMethod = handlerType.GetMethod(nameof(IEventHandler<IEvent>.HandleAsync));
            var eventHandlers = scope.ServiceProvider.GetServices(handlerType).ToList();

            var subscriptionsTasks = subscribers
                .Select(subscription => Task.Run(async() => await subscription.InvokeAsync(@event)))
                .ToArray();
            var handlersTasks = eventHandlers
                .Select(handler => Task.Run(() => handlerMethod!.Invoke(handler, [@event, default]) as Task))
                .ToArray();
            await Task.WhenAll(subscriptionsTasks.Concat(handlersTasks).ToArray());
        });
    }

    private void Unsubscribe(ISubscription subscription)
    {
        lock (_subscriptionsRepository)
        {
            _subscriptionsRepository.Remove(subscription);
        }
    }

    private class SubscriptionsRepository
    {
        private readonly List<ISubscription> _subscriptions = [];

        public void Add(ISubscription subscription)
        {
            _subscriptions.Add(subscription);
        }

        public void Remove(ISubscription subscription)
        {
            _subscriptions.Remove(subscription);
        }

        public IEnumerable<ISubscription> Find(Func<ISubscription, bool> predicate)
        {
            return _subscriptions.Where(predicate).ToArray();
        }
    }

    private interface ISubscription
    {
        public bool IsListenerOf(IEvent? @event);

        public Task InvokeAsync(IEvent @event);
    }

    private class EventSubscription<TEvent> : ISubscription, IDisposable
        where TEvent : class, IEvent
    {
        private bool _disposed;
        private readonly InMemoryBEventBus _eventBus;
        private Func<TEvent, Task>? _asyncHandler;
        private Action<TEvent>? _handler;
        
        private EventSubscription(InMemoryBEventBus eventBus)
        {
            ArgumentNullException.ThrowIfNull(eventBus);
            _eventBus = eventBus;
        }

        public static EventSubscription<TEvent> Sync(InMemoryBEventBus eventBus, Action<TEvent> handler) =>
            new(eventBus)
            {
                _handler = handler ?? throw new ArgumentNullException(nameof(handler))
            };
        
        public static EventSubscription<TEvent> Async(InMemoryBEventBus eventBus, Func<TEvent, Task> handler) =>
            new(eventBus)
            {
                _asyncHandler = handler ?? throw new ArgumentNullException(nameof(handler))
            };


        public bool IsListenerOf(IEvent? @event)
        {
            if (@event is null)
                return false;

            var subscriptionEventType = typeof(TEvent);
            return subscriptionEventType.IsInstanceOfType(@event);
        }

        public async Task InvokeAsync(IEvent @event)
        {
            if (@event is TEvent args)
            {
                if (_asyncHandler is not null)
                {
                    await _asyncHandler.Invoke(args);
                    return;
                }
            
                _handler?.Invoke(args); 
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        private void Dispose(bool disposing)
        {
            if (!disposing || _disposed)
                return;

            _eventBus.Unsubscribe(this);
            _handler = null;
            
            _disposed = true;
        }

        ~EventSubscription()
        {
            Dispose(false);
        }
    }
}