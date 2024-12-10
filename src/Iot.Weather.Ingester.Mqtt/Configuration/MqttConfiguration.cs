using System.ComponentModel.DataAnnotations;

namespace Iot.Weather.Ingester.Mqtt.Configuration;

public class MqttConfiguration
{
    [Required]
    public string? Username { get; init; }
    [Required]
    public string? Password { get; init; }
    [Required]
    public string? Server { get; init; }
    public int Port { get; init; }
    public bool UseTls { get; init; }
}