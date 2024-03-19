using AirQ.Consumer;
using gRPC.Server.GrpcServices;
using gRPC.Server.Persistence.EF;
using gRPC.Server.Persistence.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace gRPC.Server.Persistence;

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