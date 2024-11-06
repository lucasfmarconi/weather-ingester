using FluentResults;
using Iot.Weather.Ingester.InfluxDb;
using Iot.Weather.Ingester.InfluxDb.Configuration;
using Iot.Weather.Ingester.Mqtt;
using Iot.Weather.Ingester.Worker.Configuration;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;

namespace Iot.Weather.Ingester.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMqttSubscriber _mqttSubscriber;
    private readonly IInfluxDbServiceWriter _influxServiceWriter;
    private readonly IOptions<InfluxDbConfiguration> _influxDbOptions;
    private readonly IOptions<MqttConfiguration> _mqttOptions;

    public Worker(
        ILogger<Worker> logger,
        IMqttSubscriber mqttSubscriber,
        IInfluxDbServiceWriter influxServiceWriter,
        IOptions<InfluxDbConfiguration> influxDbOptions,
        IOptions<MqttConfiguration> mqttOptions)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mqttSubscriber = mqttSubscriber ?? throw new ArgumentNullException(nameof(mqttSubscriber));
        _influxServiceWriter = influxServiceWriter ?? throw new ArgumentNullException(nameof(influxServiceWriter));
        _influxDbOptions = influxDbOptions ?? throw new ArgumentNullException(nameof(influxDbOptions));
        _mqttOptions = mqttOptions ?? throw new ArgumentNullException(nameof(mqttOptions));
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
             ("humidity", humidity.Value));
        _logger.LogDebug("Humidity sensor value sent...");
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
             ("temperature", temperature.Value));
        _logger.LogDebug("Temperature sensor value sent...");
        return Task.CompletedTask;
    }

    private static Result<int> TryConvertSensorValueToInt(string value)
    {
        return int.TryParse(value, out var result) ? Result.Ok(result) : Result.Fail("Unable to convert value to int");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var humidityFuncDelegate = HumidityMessageHandler;
        var temperatureFuncDelegate = TemperatureMessageHandler;

        await _mqttSubscriber.SubscribeToTopic("sensors/air-sensors/temperature", temperatureFuncDelegate,
            stoppingToken);
        await _mqttSubscriber.SubscribeToTopic("sensors/air-sensors/humidity", humidityFuncDelegate,
            stoppingToken);
        
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
