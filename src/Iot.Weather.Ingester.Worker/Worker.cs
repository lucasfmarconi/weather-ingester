using Iot.Weather.Ingester.Mqtt;

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
        async Task MyLocalEventHandler(EventArgs e)
        {
            // Your local function logic here...
            await Task.Delay(1000); // For example, simulate some asynchronous operation
            Console.WriteLine("Local function executed.");
        }

        await _mqttSubscriber.SubscribeToTopic("/sensors/air-sensors/#", MyLocalEventHandler, stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
