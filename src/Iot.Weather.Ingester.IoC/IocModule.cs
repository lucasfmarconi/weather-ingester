﻿using InfluxDB.Client;
using Iot.Weather.Ingester.InfluxDb;
using Iot.Weather.Ingester.InfluxDb.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Iot.Weather.Ingester.IoC;

public static class IocModule
{
    public static IServiceCollection RegisterModules(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterInfluxDb(configuration);
        return services;
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
        services.AddSingleton<IInfluxDBClient>(new InfluxDBClient(influxConfig.Server, token));
        services.AddSingleton<IInfluxDbServiceWriter, InfluxDbServiceWriter>();

        return services;
    }
}
