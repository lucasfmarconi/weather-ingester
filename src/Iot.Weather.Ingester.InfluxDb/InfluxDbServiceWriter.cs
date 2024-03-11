using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace Iot.Weather.Ingester.InfluxDb;

internal sealed class InfluxDbServiceWriter(IInfluxDBClient influxDbClient) : IInfluxDbServiceWriter
{
    private readonly IInfluxDBClient _influxDbClient =
        influxDbClient ?? throw new ArgumentNullException(nameof(influxDbClient));

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

        using var writeApi = _influxDbClient.GetWriteApi();
        writeApi.WritePoint(point, bucket, organization);
    }
}