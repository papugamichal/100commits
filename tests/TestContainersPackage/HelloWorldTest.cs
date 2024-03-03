using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace TestContainers;

public class HelloWorldTest
{
    private const int ContainerPort = 8080;
    
    private IContainer _container;

    [SetUp]
    public async Task Setup()
    {
        _container = new ContainerBuilder()
            .WithImage("testcontainers/helloworld:1.1.0")
            .WithPortBinding(ContainerPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(strategy => strategy.ForPort(ContainerPort)))
            .Build();
        
        Console.WriteLine("Container status: " + _container.Health);

        await _container.StartAsync();
        
        Console.WriteLine("Container status: " + _container.Health);
    }

    [TearDown]
    public async Task Cleanup()
    {
        await _container.StopAsync();
    }
    
    [Test]
    public async Task Test1()
    {
        //Arrange
        var httpClient = new HttpClient();

        var requestUri = new UriBuilder(Uri.UriSchemeHttp, _container.Hostname, _container.GetMappedPublicPort(ContainerPort), "uuid").Uri;

        //Act
        _ = Guid.TryParse(await httpClient.GetStringAsync(requestUri)
            .ConfigureAwait(false), out var guid);
        
        //Assert
        Assert.That(guid, Is.Not.EqualTo(Guid.Empty));
    }
}