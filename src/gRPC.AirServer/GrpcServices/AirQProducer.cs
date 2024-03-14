using System.Text.Json;
using Airq.Producer;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace gRPC.Server.GrpcServices;

internal class AirQProducer : Airq.Producer.AirQProducer.AirQProducerBase
{
    public override async Task<Empty> StreamToServer(IAsyncStreamReader<AirData> requestStream, ServerCallContext context)
    {
        try
        {
            var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
            await foreach (var data in requestStream.ReadAllAsync(cts.Token))
            {
                Console.WriteLine(
                    $"Received data: {System.Text.Json.JsonSerializer.Serialize(data, options: new JsonSerializerOptions() { WriteIndented = true })}");
            }
        }
        catch (Exception e)
        {
        }

        //Server 
        return new Empty();
    }
}