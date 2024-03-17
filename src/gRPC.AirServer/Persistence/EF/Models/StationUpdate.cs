namespace gRPC.Server.Persistence.EF.Models;

public class StationUpdate
{
    public string StationName { get; set; }
    public DateTimeOffset Created { get; set; }
    public string Data { get; set; }
}