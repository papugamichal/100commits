using gRPC.Server.GrpcServices;

namespace gRPC.Server.Persistence;

internal interface IRepository
{
    Task Persist(StationName name, AirQ.Consumer.AirQMetrics metrics);
    Task<List<AirQ.Consumer.AirQMetrics>> GetLastUpdates(StationName name, int updates);
}