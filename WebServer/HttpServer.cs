using System.Net.Sockets;
using System.Text;

namespace WebServer;

public class HttpServer(string ip, int port)
{
    private readonly TcpListener _listener = new(System.Net.IPAddress.Parse(ip), port);

    public async Task Start()
    {
        _listener.Start();
        Console.WriteLine($"Http Server started on {ip}:{port}");
        while (true)
        {
            using var client = await _listener.AcceptTcpClientAsync();
            _ = HandleConnectionAsync(client);
        }

    }

    private static async Task HandleConnectionAsync(TcpClient client)
    {
        using var stream = client.GetStream();
        var buffer = new byte[4096];

        var bytesRead = await stream.ReadAsync(buffer);

        var response = BuildHttpResponse("You are connected");
        var responseBytes = Encoding.ASCII.GetBytes(response);

        await stream.WriteAsync(responseBytes);
    }

    private static string BuildHttpResponse(string content)
    {
        return $"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {content.Length}\r\n\r\n{content}";
    }
}