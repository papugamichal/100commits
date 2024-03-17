using gRPC.Server.Persistence.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace gRPC.Server.Persistence.EF;

public class AirQDbContext : DbContext
{
    private DbSet<StationUpdate> StationUpdate { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StationUpdate>()
            .ToTable("StationUpdates");
    }
}          