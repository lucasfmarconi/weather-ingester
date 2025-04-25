using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Logging;

namespace Iot.Weather.Ingester.InfluxDb;

internal sealed class InfluxDbServiceWriter(ILogger<InfluxDbServiceWriter> logger, IInfluxDBClient influxDbClient) 
    : IInfluxDbServiceWriter
{
    private readonly IInfluxDBClient _influxDbClient =
        influxDbClient ?? throw new ArgumentNullException(nameof(influxDbClient));
    
    private readonly ILogger<InfluxDbServiceWriter> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public void WriteToDataPoint(string bucket,
        string organization,
        string measurement,
        (string, string) tag,
        (string, object) field)
    {
        var point = PointData
            .Measurement(measurement)
            .Tag(tag.Item1, tag.Item2)
            .Field(field.Item1, field.Item2)
            .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

        try
        {
            using var writeApi = _influxDbClient.GetWriteApi();
            writeApi.WritePoint(point, bucket, organization);
            logger.LogDebug("Written to InfluxDB point: {point}", point.ToLineProtocol());
        }
        catch (Exception ex)
        {
            logger.LogError("Error to write point to InfluxDB: {Message}", ex.Message);
        }
    }
}