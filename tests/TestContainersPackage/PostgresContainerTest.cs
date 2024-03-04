using DotNet.Testcontainers.Containers;
using Npgsql;
using Testcontainers.PostgreSql;

namespace TestContainers;

public class PostgresContainerTest
{
    private PostgreSqlContainer  _container;

    [SetUp]
    public async Task Setup()
    {
        _container = new PostgreSqlBuilder()
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
        var connectionString = _container.GetConnectionString();
        using var npgSqlConnection = new NpgsqlConnection(connectionString);
        using var command = new NpgsqlCommand();
            
        npgSqlConnection.Open();
        command.CommandText = "SELECT 1";
        command.Connection = npgSqlConnection;
        
        //Act
        var result = await command.ExecuteNonQueryAsync();
        
        //Assert
        Assert.That(result, Is.Not.EqualTo(1));
    }
}