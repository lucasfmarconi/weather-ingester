using System.ComponentModel.DataAnnotations;

namespace Iot.Weather.Ingester.InfluxDb.Configuration;

internal class InfluxDbConfiguration()
{
    [Required]
    public string Bucket { get; init; }
    [Required]
    public string Organization { get; init; }
    [Required]
    public string Server { get; init; }
}
