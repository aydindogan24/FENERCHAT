// Amaç: TCP istemci; bağlanır, ad/mesaj gönderir, sunucudan gelenleri dinler.
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Networking
{
    public sealed class TcpChatClient : IDisposable
    {
        private readonly IPAddress serverAddress;
        private readonly int serverPort;
        private readonly Func<string> getUserName;
        private TcpClient client;
        private volatile bool running;

        public event EventHandler<string> MessageReceived;
        public bool IsConnected { get { return client != null && client.Connected; } }

        public TcpChatClient(IPAddress address, int port, Func<string> getUserName)
        {
            serverAddress = address;
            serverPort = port;
            this.getUserName = getUserName;
        }

        public async Task<bool> ConnectAsync(TimeSpan timeout)
        {
            try
            {
                Console.WriteLine($"[TCP Client] Bağlanılıyor: {serverAddress}:{serverPort}");
                client = new TcpClient();
                var connectTask = client.ConnectAsync(serverAddress, serverPort);
                var completed = await Task.WhenAny(connectTask, Task.Delay(timeout)) == connectTask;
                if (!completed) 
                {
                    Console.WriteLine("[TCP Client] Bağlantı timeout");
                    return false;
                }

                var name = getUserName();
                var nameBytes = Encoding.UTF8.GetBytes(name + "\n");
                await client.GetStream().WriteAsync(nameBytes, 0, nameBytes.Length);
                Console.WriteLine($"[TCP Client] Kullanıcı adı gönderildi: {name}");

                running = true;
                Task.Run(ReceiveLoopAsync);
                Console.WriteLine("[TCP Client] Bağlantı başarılı");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TCP Client] Bağlantı hatası: {ex.Message}");
                return false;
            }
        }

        private async Task ReceiveLoopAsync()
        {
            if (client == null) return;
            var stream = client.GetStream();
            var buffer = new byte[4096];

            try
            {
                while (running && client.Connected)
                {
                    var line = await ReadLineAsync(stream, buffer);
                    if (line == null) break;
                    
                    var message = line.Trim();
                    if (message.Length > 0)
                    {
                        Console.WriteLine($"[TCP Client] Mesaj alındı: {message}");
                        if (MessageReceived != null) MessageReceived(this, message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TCP Client] ReceiveLoop hatası: {ex.Message}");
            }
        }

        private static async Task<string> ReadLineAsync(NetworkStream stream, byte[] buffer)
        {
            var sb = new StringBuilder();
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                var chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                int nlIndex = chunk.IndexOf('\n');
                if (nlIndex >= 0)
                {
                    sb.Append(chunk.Substring(0, nlIndex));
                    return sb.ToString().TrimEnd('\r');
                }
                sb.Append(chunk);
                if (!stream.DataAvailable)
                    await Task.Yield();
            }
            return null;
        }

        public async Task SendAsync(string message)
        {
            if (client == null || !client.Connected) return;
            var bytes = Encoding.UTF8.GetBytes(message + "\n");
            await client.GetStream().WriteAsync(bytes, 0, bytes.Length);
        }

        public void Dispose()
        {
            running = false;
            try { if (client != null) client.Close(); } catch { }
            client = null;
        }
    }
}