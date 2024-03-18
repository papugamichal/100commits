using System.Collections.Concurrent;
using System.Net.Mime;
using AirQ.Consumer;
using gRPC.Server.GrpcServices;
using gRPC.Server.Persistence.EF;
using gRPC.Server.Persistence.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace gRPC.Server.Persistence;

internal interface IRepository
{
    Task Persist(StationName name, AirQ.Consumer.AirQMetrics metrics);
    Task<List<AirQ.Consumer.AirQMetrics>> GetLastUpdates(StationName name, int updates);
}

internal class DatabaseRepository(AirQDbContext context) : IRepository
{
    public async Task Persist(StationName name, AirQMetrics metrics)
    {
        var row = new StationUpdate()
        {
            StationName = name.ToString(),
            Created = DateTimeOffset.Now,
            Data = System.Text.Json.JsonSerializer.Serialize(metrics)
        };
        await context.StationUpdate.AddAsync(row);
        await context.SaveChangesAsync();
    }

    public async Task<List<AirQ.Consumer.AirQMetrics>> GetLastUpdates(StationName name, int updates)
    {
        var data = await context.StationUpdate
            .Where(s => s.StationName == name.ToString()).ToListAsync();

        var r = data
            .OrderByDescending(e => e.Created)
            .TakeLast(updates)
            .Select(u => System.Text.Json.JsonSerializer.Deserialize<AirQ.Consumer.AirQMetrics>(u.Data))
            .Where(e => e != null)
            .ToList();
        return r;
    }
}


internal class InMemoryRepository : IRepository
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