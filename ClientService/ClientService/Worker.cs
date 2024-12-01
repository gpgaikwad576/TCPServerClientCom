using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ClientService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private const string EncryptionKey = "SimpleKey"; // Define your key here

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }
    
    public static class EncryptDecrypt
    {
        // XOR Encryption and Decryption
        public static string EncryptDecryptTask(string input, string key)
        {
            StringBuilder output = new StringBuilder();
            int keyIndex = 0;

            foreach (char c in input)
            {
                output.Append((char)(c ^ key[keyIndex]));
                keyIndex = (keyIndex + 1) % key.Length;
            }

            return output.ToString();
        }
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
                string? message = "NOTHING";
                    
                using (var stream = client.GetStream())
                {
                    // Separate Task for Receiving Messages
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

                                string encryptedResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                                string response = EncryptDecrypt.EncryptDecryptTask(encryptedResponse, EncryptionKey); // Decrypt response

                                // Your requested output line
                                Console.WriteLine($"\r\nServer({message}, {DateTimeOffset.Now}): {response}");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error receiving server response.");
                                break;
                            }
                        }
                    }, stoppingToken);

                    
                    var sendingTask = Task.Run(async () =>
                    {
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

                                string encryptedMessage = EncryptDecrypt.EncryptDecryptTask(message, EncryptionKey); // Encrypt message
                                byte[] messageBytes = Encoding.UTF8.GetBytes(encryptedMessage);
                                await stream.WriteAsync(messageBytes, 0, messageBytes.Length, stoppingToken);
                                await stream.FlushAsync(stoppingToken);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error sending message to server.");
                                break;
                            }
                        }
                    }, stoppingToken);

                    // Wait for both tasks to complete
                    await Task.WhenAll(receivingTask, sendingTask);
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
