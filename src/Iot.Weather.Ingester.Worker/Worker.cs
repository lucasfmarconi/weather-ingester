using Iot.Weather.Ingester.Mqtt;
using MQTTnet;
using MQTTnet.Client;

namespace Iot.Weather.Ingester.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IMqttSubscriber _mqttSubscriber;

    public Worker(ILogger<Worker> logger, IMqttSubscriber mqttSubscriber)
    {
        _logger = logger;
        _mqttSubscriber = mqttSubscriber;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Task MessageHandler(MqttApplicationMessageReceivedEventArgs e)
        {
            _logger.LogDebug("Received application message.");
            _logger.LogDebug(e.ApplicationMessage.ConvertPayloadToString());
            return Task.CompletedTask;
        }

        Func<MqttApplicationMessageReceivedEventArgs, Task> localFunctionDelegate = MessageHandler;

        await _mqttSubscriber.SubscribeToTopic("/sensors/air-sensors/#", localFunctionDelegate, stoppingToken);
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
