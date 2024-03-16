using System.Collections.Concurrent;
using AirQ.Consumer;
using gRPC.Server.GrpcServices;

namespace gRPC.Server.Services;

internal class Repository
{
    private readonly ConcurrentDictionary<StationName, List<AirQ.Consumer.AirQMetrics>?> _store = new();
    
    public Task Persist(StationName name, AirQ.Consumer.AirQMetrics metrics)
    {
        _store.AddOrUpdate(name, new List<AirQ.Consumer.AirQMetrics> { metrics }, (stationName, list) =>
        {
            list?.Add(metrics);
            return list;
        });

        return Task.CompletedTask;
    }

    public Task<List<AirQ.Consumer.AirQMetrics>> GetLastUpdates(StationName name, int updates)
    {
        var result = _store.TryGetValue(name, out var data)
            ? data?.TakeLast(updates).ToList() ?? []
            : [];

        return Task.FromResult(result);
    }
    
}