using TCPServerService;
using TCPServerService.State;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Remove default logging providers and configure custom logging
builder.Logging.ClearProviders(); // Removes all default logging providers
builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Critical; // Logs only Critical messages
});



// Add the worker service
builder.Services.AddSingleton<DataState>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();