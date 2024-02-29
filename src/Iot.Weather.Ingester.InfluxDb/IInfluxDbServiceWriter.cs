namespace Iot.Weather.Ingester.InfluxDb;

public interface IInfluxDbServiceWriter
{
    void WriteToDataPoint(string bucket,
        string organization,
        string measurement,
        (string, string) tag,
        (string, object) field);
}