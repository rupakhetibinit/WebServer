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

    private static async Task HandleClient(TcpClient client)
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

            try
            {
                await ListenForMessages(client, stream);
            }
            catch (IOException e)
            {
                Console.WriteLine($"Client disconnected unexpectedly: {e.Message}");
            }
            catch (SocketException e)
            {
                Console.WriteLine($"Socket error: {e.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unknown error : {ex.Message}");
            }
        }
    }

    private static async Task ListenForMessages(TcpClient client, NetworkStream stream)
    {
        while (client.Connected)
        {
            var header = await ReadBytes(stream, 2);

            bool fin = (header[0] & 0x80) != 0;
            byte opcode = (byte)(header[0] & 0x0F);
            bool hasMask = (header[1] & 0x80) != 0;

            if (!hasMask) throw new Exception("Client Messages must be masked");

            ulong payloadLength = (ulong)(header[1] & 0x7F);

            if (payloadLength == 126)
            {
                var lenBytes = await ReadBytes(stream, 2);
                Array.Reverse(lenBytes);
                payloadLength = BitConverter.ToUInt16(lenBytes, 0);
            }
            else if (payloadLength == 127)
            {
                var lenBytes = await ReadBytes(stream, 8);
                Array.Reverse(lenBytes);
                payloadLength = BitConverter.ToUInt64(lenBytes, 0);
            }

            var maskingKey = await ReadBytes(stream, 4);
            var payload = await ReadBytes(stream, (int)payloadLength);

            UnmaskPayload(payload, maskingKey);

            var message = Encoding.UTF8.GetString(payload);
            Console.WriteLine($"Recieved : {message}");

        }

    }

    private static async Task<byte[]> ReadBytes(NetworkStream stream, int count)
    {
        byte[] buffer = new byte[count];
        int bytesRead = 0;
        while (bytesRead < count)
        {
            int read = await stream.ReadAsync(buffer.AsMemory(bytesRead, count - bytesRead));
            if (read == 0) throw new Exception("Connection closed prematurely");
            bytesRead += read;
        }
        return buffer;
    }

    private static void UnmaskPayload(byte[] payload, byte[] maskingKey)
    {
        for (int i = 0; i < payload.Length; i++)
        {
            payload[i] = (byte)(payload[i] ^ maskingKey[i % 4]);
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
