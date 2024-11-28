using System.Net;
using System.Net.Sockets;
using System.Text;
namespace ClientService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TCP Client is starting");

        try
        {
            using (var client = new TcpClient())
            {
                _logger.LogInformation("Connecting to server");
                await client.ConnectAsync("127.0.0.1", 5000, stoppingToken);
                _logger.LogInformation("Connected to server.");

                using (var stream = client.GetStream())
                {
                    string? message = "EMPTY";
                    var receivingTask = Task.Run(async () =>
                    {
                        while (!stoppingToken.IsCancellationRequested)
                        {
                            try
                            {
                                byte[] buffer = new byte[1024];
                                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, stoppingToken);

                                if (bytesRead == 0)
                                {
                                    _logger.LogWarning("Server closed the connection.");
                                    break;
                                }

                                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                                //Server(rsponse for which message,when response received):response
                                Console.WriteLine($"\r\nServer({message}, {DateTimeOffset.Now}): {response}"); 
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error receiving server response.");
                                break;
                            }
                        }
                    }, stoppingToken);

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            Console.Write("\r\nEnter message: ");
                            message = Console.ReadLine();

                            if (string.IsNullOrWhiteSpace(message))
                            {
                                _logger.LogInformation("Empty message, exiting");
                                break;
                            }

                            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                            await stream.WriteAsync(messageBytes, 0, messageBytes.Length, stoppingToken);
                            await stream.FlushAsync(stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error sending message to server.");
                            break;
                        }
                    }

                    await receivingTask;
                }
            }
        }
        catch (SocketException ex)
        {
            _logger.LogError(ex, "Error connecting to the server.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred.");
        }

        _logger.LogInformation("TCP Client stopped.");
    }
}