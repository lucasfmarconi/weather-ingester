using Iot.Weather.Ingester.Mqtt.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;

namespace Iot.Weather.Ingester.Mqtt;

public class MqttSubscriber : IMqttSubscriber
{
    private readonly MqttFactory _mqttFactory;
    private readonly ILogger<MqttSubscriber> _logger;
    private readonly MqttConfiguration _mqttConfiguration;

    public MqttSubscriber(MqttFactory mqttFactory, IOptions<MqttConfiguration> mqttOptions, ILogger<MqttSubscriber> logger)
    {
        _mqttFactory = mqttFactory ?? throw new ArgumentNullException(nameof(mqttFactory));
        _logger = logger;
        _mqttConfiguration = mqttOptions.Value ?? throw new ArgumentNullException(nameof(mqttOptions));
    }

    private async Task<IMqttClient> ConnectToBroker(string brokerUrl,
        string username,
        string password,
        CancellationToken cancellationToken,
        int port = 1883)
    {
        _logger.LogInformation("Connecting to broker...");
        
        var mqttClient = _mqttFactory.CreateMqttClient();

        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(brokerUrl, port).WithCredentials(username, password) // Set username and password
            .WithClientId($"WeatherIngester-{Guid.NewGuid()}")
            .WithCleanSession()
            .WithProtocolVersion(MqttProtocolVersion.V500)
            .WithTlsOptions(o =>
            {
                o.UseTls();
                o.WithAllowUntrustedCertificates();
            })
            .Build();   
        // In MQTTv5 the response contains much more information.
        var response = await mqttClient.ConnectAsync(mqttClientOptions, cancellationToken);
        if (response.ResultCode != MqttClientConnectResultCode.Success)
        {
            _logger.LogError("Failed to connect to broker.");
            _logger.LogTrace("MQTT Client response: {@response}", response);
            return mqttClient;
        }
        
        _logger.LogInformation("The MQTT client is connected.");
        return mqttClient;
    }

    public async Task SubscribeToTopic(
        string topic, Func<MqttApplicationMessageReceivedEventArgs, Task> messageCallBackDelegate,
        CancellationToken cancellationToken)
    {
        // Setup message handling before connecting so that queued messages
        // are also handled properly. When there is no event handler attached all
        // received messages get lost.
        var mqttClient = await ConnectToBroker(_mqttConfiguration.Server,
            _mqttConfiguration.Username,
            _mqttConfiguration.Password,
            cancellationToken,
            _mqttConfiguration.Port);
        mqttClient.ApplicationMessageReceivedAsync += messageCallBackDelegate;

        var mqttSubscribeOptions = _mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
                f =>
                {
                    f.WithTopic(topic);
                })
            .Build();

        await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

        _logger.LogInformation("MQTT client subscribed to topic.");
    }
}
