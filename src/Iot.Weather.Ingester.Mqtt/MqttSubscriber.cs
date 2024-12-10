using Iot.Weather.Ingester.Mqtt.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Exceptions;
using MQTTnet.Formatter;
using Polly;
using Polly.Retry;

namespace Iot.Weather.Ingester.Mqtt;

public class MqttSubscriber : IMqttSubscriber
{
    private readonly MqttFactory _mqttFactory;
    private readonly ILogger<MqttSubscriber> _logger;
    private readonly MqttConfiguration _mqttConfiguration;

    public MqttSubscriber(MqttFactory mqttFactory,
        IOptions<MqttConfiguration> mqttOptions,
        ILogger<MqttSubscriber> logger)
    {
        _mqttFactory = mqttFactory ?? throw new ArgumentNullException(nameof(mqttFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mqttConfiguration = mqttOptions.Value ?? throw new ArgumentNullException(nameof(mqttOptions));
    }

    private async Task<IMqttClient> ConnectToBroker(string? brokerUrl,
        string? username,
        string? password,
        bool useTls,
        CancellationToken cancellationToken,
        int port = 1883)
    {
        _logger.LogInformation("Connecting to broker {brokerUrl}...", brokerUrl);

        var mqttClient = _mqttFactory.CreateMqttClient();
        var mqttClientOptions = useTls ?
            new MqttClientOptionsBuilder()
                .WithTcpServer(brokerUrl, port).WithCredentials(username, password) // Set username and password
                .WithClientId($"WeatherIngester-{Guid.NewGuid()}")
                .WithCleanSession()
                .WithProtocolVersion(MqttProtocolVersion.V500)
                .WithTlsOptions(o =>
                {
                    o.UseTls();
                    o.WithAllowUntrustedCertificates();
                })
                .Build() :
            new MqttClientOptionsBuilder()
                .WithTcpServer(brokerUrl, port).WithCredentials(username, password) // Set username and password
                .WithClientId($"WeatherIngester-{Guid.NewGuid()}")
                .WithCleanSession()
                .WithProtocolVersion(MqttProtocolVersion.V500)
                .Build();

        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                Delay = TimeSpan.FromSeconds(2),
                MaxRetryAttempts = 15,
                BackoffType = DelayBackoffType.Exponential,
                MaxDelay = TimeSpan.FromMinutes(15),
                OnRetry = args =>
                {
                    _logger.LogWarning("Retry nbr {nbr} broker reconnect policy...", args.AttemptNumber);
                    return default;
                } 
            })
            .Build();

        mqttClient = await pipeline.ExecuteAsync(async token =>
        {
            var response = await mqttClient.ConnectAsync(mqttClientOptions, cancellationToken);
            if (response.ResultCode != MqttClientConnectResultCode.Success)
            {
                _logger.LogError("Failed to connect to broker.");
                _logger.LogTrace("MQTT Client response: {@response}", response);
                return mqttClient;
            }

            _logger.LogInformation("The MQTT client is connected.");
            return mqttClient;
        }, cancellationToken);
        
        mqttClient.DisconnectedAsync += async e =>
        {
            _logger.LogInformation("The MQTT client is disconnected. Reason: {reason}", e.Reason);
            _logger.LogTrace("The MQTT client is disconnected... See event details: {@event}", e);
            try
            {
                _logger.LogInformation("MQTT client is trying to reconnect to broker...");
                await ConnectToBroker(brokerUrl, username, password, useTls, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reconnect to broker.");
            }
        };

        return mqttClient;
    }

    public async Task SubscribeToTopic(
        string topic, Func<MqttApplicationMessageReceivedEventArgs, Task> messageCallBackDelegate,
        CancellationToken cancellationToken)
    {
        _logger.LogTrace("MQTT client is subscribing to topic {topic}...", topic);
        // Setup message handling before connecting so that queued messa    es
        // are also handled properly. When there is no event handler attached all
        // received messages get lost.
        var mqttClient = await ConnectToBroker(_mqttConfiguration.Server,
            _mqttConfiguration.Username,
            _mqttConfiguration.Password,
            _mqttConfiguration.UseTls,
            cancellationToken,
            _mqttConfiguration.Port);
        mqttClient.ApplicationMessageReceivedAsync += messageCallBackDelegate;

        var mqttSubscribeOptions = _mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
                f => { f.WithTopic(topic); })
            .Build();

        await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

        _logger.LogInformation("MQTT client subscribed to topic.");
    }
}