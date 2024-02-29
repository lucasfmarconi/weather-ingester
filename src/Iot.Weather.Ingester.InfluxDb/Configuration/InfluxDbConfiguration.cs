using System.ComponentModel.DataAnnotations;

namespace Iot.Weather.Ingester.InfluxDb.Configuration;

public class InfluxDbConfiguration()
{
    [Required]
    public string Bucket { get; init; }
    [Required]
    public string Organization { get; init; }
    [Required]
    public string Server { get; init; }
}
