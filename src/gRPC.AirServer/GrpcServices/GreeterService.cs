using System.Text.Json;
using AirQ.Consumer;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using AirData = Airq.Producer.AirData;

namespace gRPC.Server.GrpcServices;

public class GreeterService : Greeter.GreeterBase
{
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloReply()
        {
            Message = "Pong!"
        });
    }
}


public class ConsumerService : AirQConsumer.AirQConsumerBase
{
    public override async Task StreamUpdates(StreamRequest request, IServerStreamWriter<AirQMetrics> responseStream, ServerCallContext context)
    {
        var stationName = request.StationName;

        var random = new Random();
        for (int i = 0; i < 20; i++)
        {
            var data = new AirQMetrics
            {
                Humidity = random.NextDouble() * 10,
                Pm10 = (int) random.NextInt64(0, 150),
                No2 = (int) random.NextInt64(0, 150),
                So2 = (int) random.NextInt64(0, 150),
                Pressure = (int) random.NextInt64(990, 1050),
                Temperature = random.NextInt64(-15, 12) + random.NextDouble()
            };
            await responseStream.WriteAsync(data);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }

    public override async Task GetBidirectionalUpdatesStream(IAsyncStreamReader<StreamRequest> requestStream, IServerStreamWriter<AirQMetrics> responseStream, ServerCallContext context)
    {
        var reqId = context.GetHttpContext().TraceIdentifier;
        
        Console.WriteLine($"Request `{nameof(GetBidirectionalUpdatesStream)}`, id: `{reqId}`.");
        var requestArg = requestStream.Current?.StationName ?? string.Empty;
        
        var t1 = Task.Run(async () =>
        {
            await foreach (var item in requestStream.ReadAllAsync(context.CancellationToken))
            {
                requestArg = item.StationName;
                
                Console.WriteLine($"Request `{nameof(GetBidirectionalUpdatesStream)}`, id: `{reqId}`. New arg: {requestArg}");
            }
        });

        var t2 = Task.Run(async () =>
        {
            var random = new Random();
            while (context.CancellationToken.IsCancellationRequested == false)
            {
                var data = new AirQMetrics
                {
                    Humidity = random.NextDouble() * 10,
                    Pm10 = (int) random.NextInt64(0, 150),
                    No2 = (int) random.NextInt64(0, 150),
                    So2 = (int) random.NextInt64(0, 150),
                    Pressure = (int) random.NextInt64(990, 1050),
                    Temperature = random.NextInt64(-15, 12) + random.NextDouble()
                };
                await responseStream.WriteAsync(data);
                await Task.Delay(TimeSpan.FromSeconds(2), context.CancellationToken);
            }
        });

        await Task.WhenAll(t1, t2);
        Console.WriteLine($"Request `{nameof(GetBidirectionalUpdatesStream)}` finished, id: `{reqId}`.");
    }
}

public class AirQProducer : Airq.Producer.AirQProducer.AirQProducerBase
{
    public override async Task<Empty> StreamToServer(IAsyncStreamReader<AirData> requestStream, ServerCallContext context)
    {
        try
        {
            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            await foreach (var data in requestStream.ReadAllAsync(cts.Token))
            {
                Console.WriteLine(
                    $"Received data: {System.Text.Json.JsonSerializer.Serialize(data, options: new JsonSerializerOptions() { WriteIndented = true })}");
            }
        }
        catch (Exception e)
        {
        }

        //Server 
        return new Empty();
    }
}