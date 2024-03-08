using AirQ.Consumer;
using Grpc.Core;

namespace gRPC.Server.GrpcServices;

public class GreeterService : Greeter.GreeterBase
{
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloReply()
        {
            Message = "Pong!"
        });
    }
}


public class ConsumerService : AirQConsumer.AirQConsumerBase
{
    public override async Task StreamUpdates(StreamRequest request, IServerStreamWriter<AirQMetrics> responseStream, ServerCallContext context)
    {
        var stationName = request.StationName;

        var random = new Random();
        for (int i = 0; i < 20; i++)
        {
            var data = new AirQMetrics
            {
                Humidity = random.NextDouble() * 10,
                Pm10 = (int) random.NextInt64(0, 150),
                No2 = (int) random.NextInt64(0, 150),
                So2 = (int) random.NextInt64(0, 150),
                Pressure = (int) random.NextInt64(990, 1050),
                Temperature = random.NextInt64(-15, 12) + random.NextDouble()
            };
            await responseStream.WriteAsync(data);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}