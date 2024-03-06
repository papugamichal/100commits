// See https://aka.ms/new-console-template for more information

using Grpc.Net.Client;

Console.WriteLine("Hello, World!");


using var channel = GrpcChannel.ForAddress("https://localhost:5555");
await channel.ConnectAsync();

var client = new Greeter.GreeterClient(channel);
var response = await client.SayHelloAsync(new HelloRequest() { Name = "Ping" });

Console.WriteLine("Greeting: " + response.Message);
Console.WriteLine("Press any key to exit...");
Console.ReadKey();

