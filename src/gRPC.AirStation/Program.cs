// See https://aka.ms/new-console-template for more information

using Grpc.Net.Client;

Console.WriteLine("Hello, World!");


using var channel = GrpcChannel.ForAddress("https://localhost:5555");
await channel.ConnectAsync();

var client = new Greeter.GreeterClient(channel);
var response = await client.SayHelloAsync(new HelloRequest() { Name = "Ping" });


var producer = new AirQProducer.AirQProducerClient(channel);
var request = producer.StreamToServer();
await request.RequestStream.WriteAsync(new AirData()
{
    StationName = "Krakow S1",
    Metrics = new AirQMetrics()
    {

    }
});
await request.ResponseAsync;


Console.WriteLine("Greeting: " + response.Message);
Console.WriteLine("Press any key to exit...");
Console.ReadKey();

