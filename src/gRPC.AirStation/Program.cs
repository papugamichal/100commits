// See https://aka.ms/new-console-template for more information

using AirQ.Producer;
using Grpc.Net.Client;

Console.WriteLine("Hello, World!");

var cts = new CancellationTokenSource();

var task = Task.Run(async () =>
{
    await Task.Delay(TimeSpan.FromSeconds(5));
    
    Console.WriteLine("Connecting to the server...");
    using var channel = GrpcChannel.ForAddress("https://localhost:5555");
    await channel.ConnectAsync(cts.Token);
    Console.WriteLine("Connection established!");

    Console.WriteLine("Streaming data...");
    var producer = new AirQProducer.AirQProducerClient(channel);
    var request = producer.StreamToServer();
    while (cts.IsCancellationRequested == false)
    {
        try
        {
            var data = GatherAirQualityData();
            await request.RequestStream.WriteAsync(data, cts.Token);
            await Task.Delay(TimeSpan.FromSeconds(10), cts.Token);
        }
        catch (Exception e)
        {
            if (cts.IsCancellationRequested)
                await request.RequestStream.CompleteAsync();
            
            break;
        }
    }
    
    await request.ResponseAsync;
    Console.WriteLine("Stream stopped!");
    return;

    AirData GatherAirQualityData()
    {
        var random = new Random(12);
        return new AirData()
        {
            StationName = "Krakow",
            Metrics = new AirQMetrics()
            { 
                Humidity = Math.Round(random.NextDouble() * 10, 3),
                Pm10 = (int) random.NextInt64(0, 150),
                No2 = (int) random.NextInt64(0, 150),
                So2 = (int) random.NextInt64(0, 150),
                Pressure = (int) random.NextInt64(990, 1050),
                Temperature = Math.Round(random.NextInt64(-15, 12) + random.NextDouble(), 3)
            }
        };
    }
});


Console.WriteLine("Press any key to exit...");
Console.ReadKey();
Console.WriteLine();

cts.Cancel();
await task;
Console.WriteLine("Bye! Bye!");
