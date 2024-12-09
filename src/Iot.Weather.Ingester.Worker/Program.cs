using Iot.Weather.Ingester.Worker;
using Iot.Weather.Ingester.IoC;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(theme: AnsiConsoleTheme.Sixteen)
    .WriteTo.File("logs/day.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);

var configuration = builder.Configuration;
configuration.AddEnvironmentVariables();
builder.Services.AddHostedService<Worker>();
builder.Services.RegisterModules(configuration);
builder.Services.AddSerilog(Log.Logger);

var host = builder.Build();
host.Run();
