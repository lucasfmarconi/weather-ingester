using MQTTnet.Client;

namespace Iot.Weather.Ingester.Mqtt;

public interface IMqttSubscriber
{
    Task SubscribeToTopic(string topic, Func<MqttApplicationMessageReceivedEventArgs, Task> messageCallBackDelegate,
        CancellationToken cancellationToken);
}
