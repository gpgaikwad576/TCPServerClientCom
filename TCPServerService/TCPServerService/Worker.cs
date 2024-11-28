using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using TCPServerService.State;
using System.Text;
using TCPServerService.Helper; 

namespace TCPServerService;



public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly DataState _dataState;

    public Worker(ILogger<Worker> logger, DataState dataState)
    {
        _logger = logger;
        _dataState = dataState;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TCP Server is starting");
        TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 5000);
        server.Start();
        _logger.LogInformation("Server is listening on port 5000");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var client = await server.AcceptTcpClientAsync(stoppingToken);
                _logger.LogInformation("Client connected!");

                _ = Task.Run(async () =>
                {
                    using (var stream = client.GetStream())
                    {
                        while (!stoppingToken.IsCancellationRequested)
                        {
                            byte[] buffer = new byte[1024];
                            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, stoppingToken);
                            
                            if (bytesRead == 0) 
                                break;
                            
                            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            _logger.LogInformation($"\r\n{DateTimeOffset.Now} Client: {message}");

                            string response = "EMPTY";
                            string set = "";
                            string subSet = "";

                            if (!String.IsNullOrWhiteSpace(message))
                            {
                                int charLocation = message.IndexOf("-", StringComparison.Ordinal);

                                if (charLocation > 0)
                                {
                                    set = message.Substring(0, charLocation);
                                    subSet = message.Substring(charLocation + 1, message.Length - charLocation - 1);
                                }
                            }

                            if (_dataState._data.ContainsKey(set))
                            {
                                var listOfDictionaries = _dataState._data[set];
                                foreach (var dictionary in listOfDictionaries)
                                {
                                    if (dictionary.ContainsKey(subSet)) // Check if the subSet exists in the dictionary
                                    {
                                        int noOfTimes = dictionary[subSet]; // Get the value associated with the subSet key
                                        for (int i = 0; i < noOfTimes; i++)
                                        {
                                            response = $"{DateTimeOffset.Now}";
                                            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                                            await stream.WriteAsync(responseBytes, 0, responseBytes.Length, stoppingToken);
                                            await stream.FlushAsync(stoppingToken);

                                            // Wait 1 second before sending the next timestamp
                                            await Task.Delay(1000, stoppingToken);
                                        }
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                byte[] emptyResponseBytes = Encoding.UTF8.GetBytes(response);
                                await stream.WriteAsync(emptyResponseBytes, 0, emptyResponseBytes.Length, stoppingToken);
                                await stream.FlushAsync(stoppingToken);
                            }
                        }
                    }
                }, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in server.");
            }
        }

        server.Stop();
        _logger.LogInformation("Server stopped.");
    }
}
