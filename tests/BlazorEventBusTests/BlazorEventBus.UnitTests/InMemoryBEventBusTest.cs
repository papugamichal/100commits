using BlazorEventBus.Abstractions;
using BlazorEventBus.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorEventBus.UnitTests;

public class InMemoryBEventBusTest
{
    private InMemoryBEventBus _bus;
    private List<IDisposable> _disposables;
    private IServiceProvider _serviceProvider;

    [SetUp]
    public void Setup()
    {
        _disposables = [];
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IEventHandler<AEvent>, AEventHandler>();
        _serviceProvider = serviceCollection.BuildServiceProvider();
        _bus = new InMemoryBEventBus(_serviceProvider);
    }

    [TearDown]
    public void Teardown()
    {
        foreach (var disposable in _disposables)
            disposable.Dispose();
    }

    [Test]
    public async Task WhenEventIsPublished_DoNotWaitForHandlersToBeInvoked()
    {
        //Arrange
        var invoked = false;
        _disposables.Add(_bus.Subscribe<AEvent>(_ =>
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(2));
            invoked = true;
        }));

        //Act
        await _bus.PublishAsync(new AEvent());

        //Assert
        Assert.That(invoked, Is.False);
    }

    [Test]
    public async Task WhenEventIsPublished_AllCorrespondingHandlersConfiguredAsDelegatesAreInvoked()
    {
        //Arrange
        var invokedA1 = false;
        var invokedA2 = false;
        var invokedB = false;
        _disposables.Add(_bus.Subscribe<AEvent>(_ => invokedA1 = true));
        _disposables.Add(_bus.Subscribe<AEvent>(_ => invokedA2 = true));
        _disposables.Add(_bus.Subscribe<BEvent>(_ => invokedB = true));


        //Act
        await _bus.PublishAsync(new AEvent());
        await Task.Delay(TimeSpan.FromMilliseconds(100));

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(invokedA1, Is.True);
            Assert.That(invokedA2, Is.True);
            Assert.That(invokedB, Is.False);
        });
    }

    //Uncomment when Subscribe<TEvent>(Func<TEvent, Task> handler) is implemented
    // [Test]
    // public async Task WhenEventIsPublished_AllCorrespondingAsyncDelegateHandlersAreInvoked()
    // {
    //     //Arrange
    //     var invokedA1 = false;
    //     var invokedA2 = false;
    //     var invokedB = false;
    //     _disposables.Add(_bus.Subscribe<AEvent>(_ => Task.FromResult(invokedA1 = true)));
    //     _disposables.Add(_bus.Subscribe<AEvent>(_ => Task.FromResult(invokedA1 = true)));
    //     _disposables.Add(_bus.Subscribe<BEvent>(_ => Task.FromResult(invokedB = true)));
    //
    //     //Act
    //     await _bus.PublishAsync(new AEvent());
    //     await Task.Delay(TimeSpan.FromMilliseconds(100));
    //     
    //     //Assert
    //     Assert.That(invokedA1, Is.True);
    //     Assert.That(invokedA2, Is.True);
    //     Assert.That(invokedB, Is.False);
    // }

    [Test]
    public async Task WhenEventIsPublished_CorrespondingDelegateHandlerIsInvoked()
    {
        //Arrange
        var invokedA = false;
        var invokedB = false;
        _disposables.Add(_bus.Subscribe<AEvent>(_ => invokedA = true));
        _disposables.Add(_bus.Subscribe<BEvent>(_ => invokedB = true));


        //Act
        await _bus.PublishAsync(new AEvent());
        await Task.Delay(TimeSpan.FromMilliseconds(100));

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(invokedA, Is.True);
            Assert.That(invokedB, Is.False);
        });
    }

    [Test]
    public async Task WhenEventIsPublished_CorrespondingScopedServiceHandlerIsInvoked()
    {
        //Arrange
        AEventHandler.IsInvoked = false;

        //Act
        await _bus.PublishAsync(new AEvent());
        await Task.Delay(TimeSpan.FromMilliseconds(200));

        //Assert
        Assert.That(AEventHandler.IsInvoked, Is.True);
    }

    public class AEvent : IEvent
    {
    }

    public class BEvent : IEvent
    {
    }


    public class AEventHandler : IEventHandler<AEvent>
    {
        public static bool IsInvoked { get; set; }

        public async Task HandleAsync(AEvent @event, CancellationToken token = default)
        {
            IsInvoked = true;
        }
    }
}