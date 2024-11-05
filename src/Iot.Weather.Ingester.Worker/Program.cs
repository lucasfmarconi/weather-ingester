using Iot.Weather.Ingester.Worker;
using Iot.Weather.Ingester.IoC;

var builder = Host.CreateApplicationBuilder(args);

var configuration = builder.Configuration;
configuration.AddEnvironmentVariables();
builder.Services.AddHostedService<Worker>();
builder.Services.RegisterModules(configuration);

var host = builder.Build();
host.Run();
