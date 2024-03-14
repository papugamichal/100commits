using System.Threading.Channels;
using AirQ.Consumer;
using Grpc.Core;

namespace gRPC.Server.GrpcServices;

internal class DataProvider
{
    private readonly Dictionary<StationName, List<Channel<AirQMetrics>>> _stationUpdatesListeners = new();
    
    public IAsyncEnumerable<AirQMetrics> ProvideStreamFor(StationName name, CancellationToken contextCancellationToken)
    {
        var channel = Channel.CreateBounded<AirQMetrics>();   
        
        return null;
    }
}

internal record StationName(string Name)
{
    public string Name { get; init; } = Name.Trim().ToLower();
}

internal class AirQConsumer : AirQ.Consumer.AirQConsumer.AirQConsumerBase
{
    private readonly DataProvider _dataProvider;

    public AirQConsumer(DataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public override async Task StreamUpdates(StreamRequest request, IServerStreamWriter<AirQMetrics> responseStream, ServerCallContext context)
    {
        await foreach (var update in _dataProvider.ProvideStreamFor(new StationName(request.StationName), context.CancellationToken))
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
            await foreach (var item in requestStream.ReadAllAsync(context.CancellationToken))
            {
                if (item.StationName == stationName)
                    continue;

                stationUpdatesTokenSource.Cancel();
                stationName = item.StationName;
                stationUpdatesTokenSource = new CancellationTokenSource();
                
                Console.WriteLine($"Request `{nameof(GetBidirectionalUpdatesStream)}`, id: `{reqId}`. New arg: {stationName}");
            }
        });

        var responseStreamTask = Task.Run(async () =>
        {
            while (context.CancellationToken.IsCancellationRequested != false)
            {
                var linkedTokenSource =
                    CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken,
                        stationUpdatesTokenSource.Token);
                await foreach (var update in _dataProvider.ProvideStreamFor(new StationName(stationName),
                                   linkedTokenSource.Token))
                {
                    await responseStream.WriteAsync(update, context.CancellationToken);
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