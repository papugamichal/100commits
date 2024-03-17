using AirQ.Consumer;
using Grpc.Core;
using gRPC.Server.Services;

namespace gRPC.Server.GrpcServices;

internal record StationName
{
    public StationName(string name)
    {
        Name = name.Trim().ToLower();
    }

    public string Name { get; init; } 

    public void Deconstruct(out string Name)
    {
        Name = this.Name;
    }
}

internal class AirQConsumer(DataProvider dataProvider) : AirQ.Consumer.AirQConsumer.AirQConsumerBase
{
    public override async Task StreamUpdates(StreamRequest request, IServerStreamWriter<AirQ.Consumer.AirQMetrics> responseStream, ServerCallContext context)
    {
        await foreach (var update in dataProvider.ProvideStreamFor(new StationName(request.StationName), context.CancellationToken))
        {
            await responseStream.WriteAsync(update);
        }
    }

    public override async Task GetBidirectionalUpdatesStream(IAsyncStreamReader<StreamRequest> requestStream, IServerStreamWriter<AirQMetrics> responseStream, ServerCallContext context)
    {
        var reqId = context.GetHttpContext().TraceIdentifier;
        Console.WriteLine($"Request `{nameof(GetBidirectionalUpdatesStream)}`, id: `{reqId}`.");

        var stationName = requestStream.Current?.StationName ?? string.Empty;
        var stationUpdatesTokenSource = new CancellationTokenSource();
        
        var requestStreamTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var item in requestStream.ReadAllAsync(context.CancellationToken))
                {
                    if (item.StationName == stationName)
                        continue;

                    stationName = item.StationName;
                    await stationUpdatesTokenSource.CancelAsync();

                    Console.WriteLine(
                        $"Request `{nameof(GetBidirectionalUpdatesStream)}`, id: `{reqId}`. New arg: {stationName}");
                }
            }
            catch (OperationCanceledException e)
            {
                if (context.CancellationToken.IsCancellationRequested)
                    Console.WriteLine($"GRPC request `{reqId}` cancelled");
            }
        });

        var responseStreamTask = Task.Run(async () =>
        {
            while (context.CancellationToken.IsCancellationRequested == false)
            {
                try
                {
                    var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken, stationUpdatesTokenSource.Token);
                    await foreach (var update in dataProvider.ProvideStreamFor(new StationName(stationName), linkedTokenSource.Token))
                    {
                        await responseStream.WriteAsync(update, context.CancellationToken);
                    }
                }
                catch (OperationCanceledException e)
                {
                    if (context.CancellationToken.IsCancellationRequested)
                        Console.WriteLine($"GRPC request `{reqId}` cancelled");

                    if (stationUpdatesTokenSource.IsCancellationRequested)
                    {
                        Console.WriteLine($"Req: `{reqId}` station changed");
                        stationUpdatesTokenSource = new CancellationTokenSource();
                    }
                }
            }
        });


        await Task.WhenAll(requestStreamTask, responseStreamTask);
        Console.WriteLine($"Request `{nameof(GetBidirectionalUpdatesStream)}` finished, id: `{reqId}`.");
        
        // var t2 = Task.Run(async () =>
        // {
        //     var random = new Random();
        //     while (context.CancellationToken.IsCancellationRequested == false)
        //     {
        //         var data = new AirQMetrics
        //         {
        //             Humidity = random.NextDouble() * 10,
        //             Pm10 = (int) random.NextInt64(0, 150),
        //             No2 = (int) random.NextInt64(0, 150),
        //             So2 = (int) random.NextInt64(0, 150),
        //             Pressure = (int) random.NextInt64(990, 1050),
        //             Temperature = random.NextInt64(-15, 12) + random.NextDouble()
        //         };
        //         await responseStream.WriteAsync(data);
        //         await Task.Delay(TimeSpan.FromSeconds(2), context.CancellationToken);
        //     }
        // });
    }
}