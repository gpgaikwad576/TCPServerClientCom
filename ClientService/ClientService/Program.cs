using ClientService;
using ClientService.Helper;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);


builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Critical; // Logs only Critical messages
});

// Add the worker service
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
