using System.Threading.Channels;

namespace gRPC.Server.GrpcServices;

internal sealed class DataProvider
{
    private readonly Dictionary<StationName, List<Channel<AirQ.Consumer.AirQMetrics>>> _stationUpdatesListeners = new();

    public async Task ProvideStationUpdate(StationName name, AirQ.Consumer.AirQMetrics update)
    {
        Channel<AirQ.Consumer.AirQMetrics>[] listeners;

        lock (_stationUpdatesListeners)
        {
            listeners = _stationUpdatesListeners.TryGetValue(name, out var data)
                ? data.ToArray()
                : Array.Empty<Channel<AirQ.Consumer.AirQMetrics>>();
        }
        await Task.WhenAll(listeners.Select(listener => listener.Writer.WriteAsync(update).AsTask()));
    }
    
    public IAsyncEnumerable<AirQ.Consumer.AirQMetrics> ProvideStreamFor(StationName name, CancellationToken contextCancellationToken)
    {
        var subscription = Channel.CreateUnbounded<AirQ.Consumer.AirQMetrics>();
        
        lock (_stationUpdatesListeners)
        {
            if (_stationUpdatesListeners.TryGetValue(name, out var listeners))
            {
                listeners.Add(subscription);

            }
            else
            {
                _stationUpdatesListeners.Add(name, new List<Channel<AirQ.Consumer.AirQMetrics>> { subscription });
            }
        }
        
        contextCancellationToken.Register(() =>
        {
            lock (_stationUpdatesListeners)
            {
                if (_stationUpdatesListeners.TryGetValue(name, out var list))
                    list.Remove(subscription);
            }
        });

        return subscription.Reader.ReadAllAsync(contextCancellationToken);
    }
}