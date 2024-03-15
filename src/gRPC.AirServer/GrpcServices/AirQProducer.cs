using System.Text.Json;
using AirQ.Producer;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace gRPC.Server.GrpcServices;

internal class AirQProducer(DataProvider provider) : AirQ.Producer.AirQProducer.AirQProducerBase
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

                await provider.ProvideStationUpdate(new StationName(data.StationName), ToConsumer(data.Metrics));
            }
        }
        catch (Exception e)
        {
        }

        //Server 
        return new Empty();
    }

    private static AirQ.Consumer.AirQMetrics ToConsumer(AirQMetrics dataMetrics)
    {
        return new AirQ.Consumer.AirQMetrics()
        {
            Humidity = dataMetrics.Humidity,
            No2 = dataMetrics.No2,
            Pressure = dataMetrics.Pressure,
            Temperature = dataMetrics.Temperature,
            Pm10 = dataMetrics.Pm10,
            So2 = dataMetrics.So2
        };
    }
}