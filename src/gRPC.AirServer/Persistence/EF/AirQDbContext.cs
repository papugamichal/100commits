using gRPC.Server.Persistence.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace gRPC.Server.Persistence.EF;

public class AirQDbContext : DbContext
{    
    public AirQDbContext(DbContextOptions<AirQDbContext> options) : base(options)
    {
    }
    
    public DbSet<StationUpdate> StationUpdate { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StationUpdate>()
            .HasKey(e => e.Id);
        
        modelBuilder.Entity<StationUpdate>()
            .ToTable("StationUpdates");
    }
}          