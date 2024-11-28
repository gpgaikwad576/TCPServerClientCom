using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCPServerService;

public class TcpServer
{
    private TcpListener _tcpListener;

    public TcpServer()
    {
        StartServer();
    }

    private void StartServer()
    {
        var port = 13000;
        var hostAddress = IPAddress.Parse("127.0.0.1"); 
        _tcpListener = new TcpListener(hostAddress, port);
        _tcpListener.Start();
        
        byte[] buffer = new byte[1024];
        string receivedMessage = string.Empty;
        
        using TcpClient client = _tcpListener.AcceptTcpClient();
        var tcpStream = client.GetStream();
        int readTotal;
        while ((readTotal = tcpStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            receivedMessage = Encoding.UTF8.GetString(buffer, 0, readTotal);
            var response = Encoding.UTF8.GetBytes("I have received a message"+receivedMessage+"\r\n");
            tcpStream.Write(response, 0, response.Length);
        }
    }
}