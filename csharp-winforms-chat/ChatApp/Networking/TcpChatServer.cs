// Amaç: TCP sunucu; çoklu istemciyi yönetir ve mesajları yayınlar.
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Networking
{
    public sealed class TcpChatServer
    {
        private readonly TcpListener listener;
        private readonly ConcurrentDictionary<TcpClient, string> clients = new ConcurrentDictionary<TcpClient, string>();
        private volatile bool running;

        public event EventHandler<string> MessageReceived;
        public event EventHandler<string> ClientConnected;
        public event EventHandler<string> ClientDisconnected;

        public TcpChatServer(IPAddress ipAddress, int port)
        {
            listener = new TcpListener(ipAddress, port);
        }

        public void Start()
        {
            if (running) return;
            running = true;
            listener.Start();

            Task.Run(async () =>
            {
                while (running)
                {
                    try
                    {
                        var client = await listener.AcceptTcpClientAsync();
                        HandleClientAsync(client);
                    }
                    catch
                    {
                        await Task.Delay(50);
                    }
                }
            });
        }

        public void Stop()
        {
            running = false;
            try { listener.Stop(); } catch { }
            foreach (var kvp in clients)
            {
                try { kvp.Key.Close(); } catch { }
            }
            clients.Clear();
        }

        private async void HandleClientAsync(TcpClient client)
        {
            string clientName = null;
            try
            {
                var stream = client.GetStream();
                var buffer = new byte[4096];

                var nameLine = await ReadLineAsync(stream, buffer);
                clientName = string.IsNullOrWhiteSpace(nameLine) ? "Kullanıcı" : nameLine.Trim();
                clients[client] = clientName;
                if (ClientConnected != null) ClientConnected(this, clientName);

                Broadcast("[Bilgi] " + clientName + " sohbete katıldı.");

                while (client.Connected)
                {
                    var line = await ReadLineAsync(stream, buffer);
                    if (line == null) break;

                    var message = line.Trim();
                    if (message.Length == 0) continue;

                    if (MessageReceived != null) MessageReceived(this, message);
                    Broadcast(message, client);
                }
            }
            catch
            {
            }
            finally
            {
                if (clientName != null)
                {
                    string removed;
                    clients.TryRemove(client, out removed);
                    if (ClientDisconnected != null) ClientDisconnected(this, clientName);
                    Broadcast("[Bilgi] " + clientName + " ayrıldı.");
                }
                try { client.Close(); } catch { }
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

        public void Broadcast(string message, TcpClient except = null)
        {
            var data = Encoding.UTF8.GetBytes(message + "\n");
            foreach (var kvp in clients)
            {
                var c = kvp.Key;
                if (except != null && object.ReferenceEquals(c, except)) continue;
                try
                {
                    if (c.Connected)
                    {
                        c.GetStream().Write(data, 0, data.Length);
                    }
                }
                catch { }
            }
        }
    }
}