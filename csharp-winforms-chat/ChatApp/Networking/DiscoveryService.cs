// Amaç: UDP broadcast ile sunucu keşfi; timeout'u Task.WhenAny ile yapar (.NET Framework uyumlu).
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Networking
{
    public sealed class DiscoveryService : IDisposable
    {
        private readonly int discoveryPort;
        private readonly int tcpPort;
        private UdpClient listener;
        private bool isServerResponderRunning;

        private const string DiscoveryRequest = "CHAT_DISCOVERY_REQ";
        private const string DiscoveryResponsePrefix = "CHAT_DISCOVERY_RES:";

        public DiscoveryService(int discoveryPort, int tcpPort)
        {
            this.discoveryPort = discoveryPort;
            this.tcpPort = tcpPort;
            StartResponder();
        }

        public async Task<IPEndPoint> TryDiscoverServerAsync(TimeSpan timeout)
        {
            using (var client = new UdpClient(AddressFamily.InterNetwork))
            {
                client.EnableBroadcast = true;
                var requestBytes = Encoding.UTF8.GetBytes(DiscoveryRequest);
                var broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, discoveryPort);

                await client.SendAsync(requestBytes, requestBytes.Length, broadcastEndPoint);

                try
                {
                    var receiveTask = client.ReceiveAsync();
                    var delayTask = Task.Delay(timeout);
                    var done = await Task.WhenAny(receiveTask, delayTask);
                    if (done == receiveTask)
                    {
                        var result = receiveTask.Result;
                        var response = Encoding.UTF8.GetString(result.Buffer);
                        if (response.StartsWith(DiscoveryResponsePrefix, StringComparison.Ordinal))
                        {
                            var parts = response.Split(':');
                            int port;
                            if (parts.Length == 2 && int.TryParse(parts[1], out port))
                            {
                                return new IPEndPoint(result.RemoteEndPoint.Address, port);
                            }
                        }
                    }
                }
                catch
                {
                }
            }
            return null;
        }

        private void StartResponder()
        {
            listener = new UdpClient(new IPEndPoint(IPAddress.Any, discoveryPort));
            isServerResponderRunning = true;

            Task.Run(async () =>
            {
                while (isServerResponderRunning)
                {
                    try
                    {
                        var result = await listener.ReceiveAsync();
                        var text = Encoding.UTF8.GetString(result.Buffer);
                        if (text == DiscoveryRequest)
                        {
                            var response = DiscoveryResponsePrefix + tcpPort;
                            var bytes = Encoding.UTF8.GetBytes(response);
                            await listener.SendAsync(bytes, bytes.Length, result.RemoteEndPoint);
                        }
                    }
                    catch
                    {
                        await Task.Delay(50);
                    }
                }
            });
        }

        public void Dispose()
        {
            isServerResponderRunning = false;
            try { if (listener != null) listener.Close(); } catch { }
            listener = null;
        }
    }
}