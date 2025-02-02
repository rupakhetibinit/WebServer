using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace WSServer;

public class Server(string iPAddress, ushort port)
{
  private readonly TcpListener _listener = new(IPAddress.Parse(iPAddress), port);
  public readonly IPAddress ipAddress = IPAddress.Parse(iPAddress);
  public readonly ushort port = port;

  public async Task Start()
  {
    _listener.Start();
    Console.WriteLine($"Websocket Server started on {ipAddress}:{port}");
    while (true)
    {
      var client = await _listener.AcceptTcpClientAsync();
      _ = HandleClient(client);
    }
  }

  private async Task HandleClient(TcpClient client)
  {
    using var stream = client.GetStream();
    byte[] buffer = new byte[1024];

    int bytesRead = await stream.ReadAsync(buffer);
    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
    if (request.Contains("Upgrade: websocket"))
    {

      string websocketKey = ExtractKey(request);
      if (string.IsNullOrEmpty(websocketKey))
      {
        Console.WriteLine("Websocket key not found");
        return;
      }
      string websocketKeyResponse = CreateWebsocketResponseKey(websocketKey);
      string response = CraftResponse(websocketKeyResponse);

      await stream.WriteAsync(Encoding.ASCII.GetBytes(response));
      Console.WriteLine($"Sent handshake response: \n{response}");

      await ListenForMessages(client, stream);
    }
  }

  private async Task ListenForMessages(TcpClient client, NetworkStream stream)
  {
    byte[] buffer = new byte[1024];

    while (client.Connected)
    {
      int bytesRead = await stream.ReadAsync(buffer);
      if (bytesRead == 0) break;
      Console.WriteLine(bytesRead.ToString());
    }
  }


  //       HTTP/1.1 101 Switching Protocols
  //       Upgrade: websocket
  //       Connection: Upgrade
  //       Sec-WebSocket Accept: s3pPLMBiTxaQ9kYGzzhZRbK+xOo=
  private static string CraftResponse(string websocketKeyResponse)
  {
    string response = "HTTP/1.1 101 Switching Protocols\r\n" +
                      "Upgrade: websocket\r\n" +
                      "Connection: Upgrade\r\n" +
                      "Sec-Websocket-Accept: " + websocketKeyResponse + "\r\n\r\n";
    return response;
  }

  private static string CreateWebsocketResponseKey(string websocketKey)
  {
    // Websocket GUID
    string magic = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
    string combined = websocketKey + magic;
    byte[] output = SHA1.HashData(Encoding.UTF8.GetBytes(combined));
    return Convert.ToBase64String(output);
  }

  private static string ExtractKey(string request)
  {
    string websocketKeyHeader = "Sec-WebSocket-Key:";
    var start = request.IndexOf(websocketKeyHeader);
    if (start == -1) return string.Empty;

    start += websocketKeyHeader.Length;
    int end = request.IndexOf("\r\n", start);
    if (end == -1) return string.Empty;

    return request.Substring(start, end - start).Trim();
  }
}
