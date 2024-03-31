namespace Iot.Weather.Ingester.Mqtt;

public interface IMqttSubscriber
{
    Task SubscribeToTopic(string topic, Func<EventArgs, Task> messageCallBackDelegate,
        CancellationToken cancellationToken);
}
