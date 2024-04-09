using BlazorEventBus.Abstractions;

namespace BlazorEventBus.InMemory;

public sealed class InMemoryBEventBus : IBEventBus
{
    private readonly SubscriptionsRepository _subscriptionsRepository = new SubscriptionsRepository();

    public Task PublishAsync(IEnumerable<IEvent> events, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public Task PublishAsync(IEvent @event, CancellationToken token = default)
    {
        throw new NotImplementedException();
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
        public bool IsListener(IEvent? @event);
        
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

        public bool IsListener(IEvent? @event)
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