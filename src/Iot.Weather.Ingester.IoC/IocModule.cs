using InfluxDB.Client;
using InfluxDB.Client.Core;
using Iot.Weather.Ingester.InfluxDb;
using Iot.Weather.Ingester.InfluxDb.Configuration;
using Iot.Weather.Ingester.Mqtt;
using Iot.Weather.Ingester.Mqtt.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;

namespace Iot.Weather.Ingester.IoC;

public static class IocModule
{
    public static IServiceCollection RegisterModules(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterInfluxDb(configuration);
        services.RegisterMqtt(configuration);
        return services;
    }

    private static void RegisterMqtt(this IServiceCollection services, IConfiguration configuration)
    {
        var configSection = configuration.GetSection("Mqtt");
        var mqttConfiguration = configSection.Get<MqttConfiguration>();
        services.AddOptions<MqttConfiguration>().Bind(configSection);
        ArgumentNullException.ThrowIfNull(mqttConfiguration);
        services.AddSingleton(new MqttFactory());
        services.AddSingleton<IMqttSubscriber, MqttSubscriber>();
    }

    private static IServiceCollection RegisterInfluxDb(this IServiceCollection services,
        IConfiguration configuration)
    {
        // You can generate an API token from the "API Tokens Tab" in the UI
        var token = Environment.GetEnvironmentVariable("INFLUX_TOKEN")!;
        ArgumentNullException.ThrowIfNull(token);

        var configSection = configuration.GetSection("InfluxDB");
        var influxConfig = configSection.Get<InfluxDbConfiguration>();
        ArgumentNullException.ThrowIfNull(influxConfig);

        services.AddOptions<InfluxDbConfiguration>().Bind(configSection);
        var influxClientOptions = new InfluxDBClientOptions(influxConfig.Server)
        {
            LogLevel = LogLevel.Basic,
            Token = token
        };
        services.AddSingleton<IInfluxDBClient>(new InfluxDBClient(influxClientOptions));
        services.AddSingleton<IInfluxDbServiceWriter, InfluxDbServiceWriter>();

        return services;
    }
}
