
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;

namespace Iot.Weather.Ingester.Mqtt;

public class MqttSubscriber : IMqttSubscriber
{
    private readonly MqttFactory _mqttFactory;

    public MqttSubscriber(MqttFactory mqttFactory)
    {
        _mqttFactory = mqttFactory;
    }

    private async Task<IMqttClient> ConnectToBroker(string brokerUrl, CancellationToken cancellationToken, int port = 1883)
    {
        var mqttClient = _mqttFactory.CreateMqttClient();

        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(brokerUrl, port) // MQTT broker address and port
                                            // .WithCredentials(username, password) // Set username and password
            .WithClientId("WeatherIngester-" + Guid.NewGuid().ToString())
            .WithCleanSession()
            .WithProtocolVersion(MqttProtocolVersion.V500)
            .Build();

        // In MQTTv5 the response contains much more information.
        var response = await mqttClient.ConnectAsync(mqttClientOptions, cancellationToken);

        Console.WriteLine("The MQTT client is connected.");

        response.DumpToConsole();

        return mqttClient;
    }

    public async Task SubscribeToTopic(
        string topic, Func<MqttApplicationMessageReceivedEventArgs, Task> messageCallBackDelegate,
        CancellationToken cancellationToken)
    {
        // Setup message handling before connecting so that queued messages
        // are also handled properly. When there is no event handler attached all
        // received messages get lost.
        var mqttClient = await ConnectToBroker("broker.emqx.io", cancellationToken);
        mqttClient.ApplicationMessageReceivedAsync += messageCallBackDelegate;
        // mqttClient.ApplicationMessageReceivedAsync += e =>
        // {
        //     e.DumpToConsole();
        //     Console.WriteLine("Received application message.");
        //     Console.WriteLine(e.ApplicationMessage.ConvertPayloadToString());
        //     return Task.CompletedTask;
        // };

        var mqttSubscribeOptions = _mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
                f =>
                {
                    f.WithTopic(topic);
                })
            .Build();

        await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);

        Console.WriteLine("MQTT client subscribed to topic.");
    }
}
