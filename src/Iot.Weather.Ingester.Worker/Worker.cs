using FluentResults;
using InfluxDB.Client.Core.Flux.Internal;
using Iot.Weather.Ingester.InfluxDb;
using Iot.Weather.Ingester.InfluxDb.Configuration;
using Iot.Weather.Ingester.Mqtt;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;

namespace Iot.Weather.Ingester.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMqttSubscriber _mqttSubscriber;
    private readonly IInfluxDbServiceWriter _influxServiceWriter;
    private IOptions<InfluxDbConfiguration> _influxDbOptions;

    public Worker(
        ILogger<Worker> logger,
        IMqttSubscriber mqttSubscriber,
        IInfluxDbServiceWriter influxServiceWriter,
        IOptions<InfluxDbConfiguration> influxDbOptions)
    {
        _logger = logger;
        _mqttSubscriber = mqttSubscriber;
        _influxServiceWriter = influxServiceWriter;
        _influxDbOptions = influxDbOptions;
    }

    private Task HumidityMessageHandler(MqttApplicationMessageReceivedEventArgs e)
    {
        var sensorValue = e.ApplicationMessage.ConvertPayloadToString();
        var humidity = TryConvertSensorValueToInt(sensorValue);
        if (humidity.IsFailed)
        {
            _logger.LogWarning("Bad received humidity: {value}", sensorValue);
            return Task.CompletedTask;
        }

        _logger.LogDebug("Received humidity: {value}", sensorValue);
        _influxServiceWriter.WriteToDataPoint(
             _influxDbOptions.Value.Bucket,
             _influxDbOptions.Value.Organization,
             "air-sensors",
             ("dht11", "humidity"),
             ("value", sensorValue));
        _logger.LogDebug("Humidity sensor value successfully wrote.");
        return Task.CompletedTask;
    }

    private Task TemperatureMessageHandler(MqttApplicationMessageReceivedEventArgs e)
    {
        var sensorValue = e.ApplicationMessage.ConvertPayloadToString();
        var temperature = TryConvertSensorValueToInt(sensorValue);
        if (temperature.IsFailed)
        {
            _logger.LogWarning("Bad received temperature: {value}", sensorValue);
            return Task.CompletedTask;
        }

        _logger.LogDebug("Received temperature: {value}", sensorValue);
        _influxServiceWriter.WriteToDataPoint(
             _influxDbOptions.Value.Bucket,
             _influxDbOptions.Value.Organization,
             "air-sensors",
             ("dht11", "temperature"),
             ("value", sensorValue));
        _logger.LogDebug("Temperature sensor value successfully wrote.");
        return Task.CompletedTask;
    }

    private Result<int> TryConvertSensorValueToInt(string value)
    {
        if (int.TryParse(value, out int result))
        {
            return Result.Ok(result);
        }
        return Result.Fail("Unable to convert value to int");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Func<MqttApplicationMessageReceivedEventArgs, Task> humidityFuncDelegate = HumidityMessageHandler;
        Func<MqttApplicationMessageReceivedEventArgs, Task> TemperatureFuncDelegate = TemperatureMessageHandler;

        await _mqttSubscriber.SubscribeToTopic("/sensors/air-sensors/dht11/temperature", TemperatureFuncDelegate, stoppingToken);
        await _mqttSubscriber.SubscribeToTopic("/sensors/air-sensors/dht11/humidity", humidityFuncDelegate, stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(15000, stoppingToken);
        }
    }
}
