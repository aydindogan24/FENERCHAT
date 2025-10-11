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
                client = new TcpClient();
                var connectTask = client.ConnectAsync(serverAddress, serverPort);
                var completed = await Task.WhenAny(connectTask, Task.Delay(timeout)) == connectTask;
                if (!completed) return false;

                var name = getUserName();
                var nameBytes = Encoding.UTF8.GetBytes(name + "\n");
                await client.GetStream().WriteAsync(nameBytes, 0, nameBytes.Length);

                running = true;
                Task.Run(ReceiveLoopAsync);
                return true;
            }
            catch
            {
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
                    var bytes = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytes <= 0) break;
                    var text = Encoding.UTF8.GetString(buffer, 0, bytes);
                    var lines = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        if (MessageReceived != null) MessageReceived(this, line.TrimEnd('\r'));
                    }
                }
            }
            catch
            {
            }
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