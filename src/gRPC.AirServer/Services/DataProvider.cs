using System.Threading.Channels;
using AirQ.Producer;
using gRPC.Server.GrpcServices;
using gRPC.Server.Persistence;

namespace gRPC.Server.Services;

internal sealed class DataProvider(IRepository repository)
{
    private static readonly Dictionary<StationName, List<Channel<AirQ.Consumer.AirQMetrics>>> StationUpdatesListeners = new();

    /// <summary>
    /// Air station provides to server update package
    /// </summary>
    /// <param name="name"></param>
    /// <param name="update"></param>
    public async Task ProvideStationUpdate(StationName name, AirQ.Producer.AirQMetrics update)
    {
        var updateForConsumer = ToConsumer(update);
        await repository.Persist(name, updateForConsumer);
        
        Channel<AirQ.Consumer.AirQMetrics>[] listeners;
        lock (StationUpdatesListeners)
        {
            listeners = StationUpdatesListeners.TryGetValue(name, out var data)
                ? data.ToArray()
                : Array.Empty<Channel<AirQ.Consumer.AirQMetrics>>();
        }
        await Task.WhenAll(listeners.Select(listener => listener.Writer.WriteAsync(updateForConsumer).AsTask()));
    }
    
    /// <summary>
    /// Server provides updates stream of selected station
    /// </summary>
    /// <param name="name"></param>
    /// <param name="contextCancellationToken"></param>
    /// <returns></returns>
    public IAsyncEnumerable<AirQ.Consumer.AirQMetrics> ProvideStreamFor(StationName name, CancellationToken contextCancellationToken)
    {
        var subscription = Channel.CreateUnbounded<AirQ.Consumer.AirQMetrics>();
        
        lock (StationUpdatesListeners)
        {
            if (StationUpdatesListeners.TryGetValue(name, out var listeners))
            {
                listeners.Add(subscription);

            }
            else
            {
                StationUpdatesListeners.Add(name, new List<Channel<AirQ.Consumer.AirQMetrics>> { subscription });
            }
        }
        
        contextCancellationToken.Register(() =>
        {
            lock (StationUpdatesListeners)
            {
                if (StationUpdatesListeners.TryGetValue(name, out var list))
                    list.Remove(subscription);
            }
        });

        return subscription.Reader.ReadAllAsync(contextCancellationToken);
    }
    
    private static AirQ.Consumer.AirQMetrics ToConsumer(AirQMetrics dataMetrics)
    {
        return new AirQ.Consumer.AirQMetrics
        {
            Humidity = dataMetrics.Humidity,
            No2 = dataMetrics.No2,
            Pressure = dataMetrics.Pressure,
            Temperature = dataMetrics.Temperature,
            Pm10 = dataMetrics.Pm10,
            So2 = dataMetrics.So2
        };
    }
}