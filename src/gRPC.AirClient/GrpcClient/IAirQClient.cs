using System.Threading.Channels;
using AirQ.Consumer;

namespace gRPC.AirClient.GrpcClient;

public interface IAirQClient
{
    IAsyncEnumerable<AirQ.Consumer.AirQMetrics> GetDataAsync();
}

public class AirQClient : IAirQClient
{
    
    public IAsyncEnumerable<AirQMetrics> GetDataAsync()
    {
        return Array.Empty<AirQMetrics>().AsParallel();
    }
}