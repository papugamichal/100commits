using BlazorEventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorEventBus.InMemory;

public sealed class InMemoryBEventBus : IBEventBus
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

    private Task PublishAsync(IEvent? @event)
    {
        if (@event is null)
            return Task.CompletedTask;

        List<ISubscription> subscribers;
        lock (_subscriptionsRepository)
        {
            subscribers = _subscriptionsRepository.Find(subscription => subscription.IsListenerOf(@event)).ToList();
        }

        _ = Task.Run(() =>
        {
            using var scope = _serviceProvider.CreateScope();
            var handlerType = typeof(IEventHandler<>).MakeGenericType(@event.GetType());
            var handlerMethod = handlerType.GetMethod(nameof(IEventHandler<IEvent>.HandleAsync));
            var eventHandlers = scope.ServiceProvider.GetServices(handlerType).ToList();

            var subscriptionsTasks = subscribers.Select(subscription => Task.Run(() => subscription.Invoke(@event))).ToArray();
            var handlersTasks = eventHandlers.Select(handler => Task.Run(() => handlerMethod!.Invoke(handler, [@event, (CancellationToken)(default)]) as Task)).ToArray();
            Task.WaitAll(subscriptionsTasks.Concat(handlersTasks).ToArray());
        });
        
        return Task.CompletedTask;
    }

    public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class, IEvent
    {
        lock (_subscriptionsRepository)
        {
            var subscription = new EventSubscription<TEvent>(this, handler);
            _subscriptionsRepository.Add(subscription);
            return subscription;
        }
    }

    public IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class, IEvent
    {
        throw new NotImplementedException();
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

        public void Add(ISubscription subscription) =>
            _subscriptions.Add(subscription);
        
        public void Remove(ISubscription subscription) =>
            _subscriptions.Remove(subscription);
        
        public IEnumerable<ISubscription> Find(Func<ISubscription, bool> predicate) =>
            _subscriptions.Where(predicate).ToArray();
    }

    private interface ISubscription
    {
        public bool IsListenerOf(IEvent? @event);
        
        public void Invoke(IEvent @event);
    }

    private class EventSubscription<TEvent> : ISubscription, IDisposable
        where TEvent : class, IEvent
    {
        private readonly InMemoryBEventBus _eventBus;
        private Action<TEvent> _handler;

        public EventSubscription(InMemoryBEventBus eventBus, Action<TEvent> handler)
        {
            ArgumentNullException.ThrowIfNull(eventBus);
            ArgumentNullException.ThrowIfNull(handler);

            _eventBus = eventBus;
            _handler = handler;
        }

        public bool IsListenerOf(IEvent? @event)
        {
            if (@event is null)
                return false;

            var subscriptionEventType = typeof(TEvent);
            return subscriptionEventType.IsInstanceOfType(@event);
        }

        public void Invoke(IEvent @event) =>
            _handler.Invoke(@event as TEvent);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            _eventBus.Unsubscribe(this);
            _handler = null;
        }

        ~EventSubscription() { Dispose(false); }
    }
}