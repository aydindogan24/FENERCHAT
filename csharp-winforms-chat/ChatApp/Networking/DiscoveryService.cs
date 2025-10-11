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
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                
                var requestBytes = Encoding.UTF8.GetBytes(DiscoveryRequest);
                
                // Hem broadcast hem de local subnet'e gönder
                var broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, discoveryPort);
                var localSubnetEndPoint = GetLocalSubnetBroadcast();

                Console.WriteLine($"[Discovery] UDP broadcast gönderiliyor: {broadcastEndPoint}");
                await client.SendAsync(requestBytes, requestBytes.Length, broadcastEndPoint);
                
                if (localSubnetEndPoint != null)
                {
                    Console.WriteLine($"[Discovery] Local subnet'e gönderiliyor: {localSubnetEndPoint}");
                    await client.SendAsync(requestBytes, requestBytes.Length, localSubnetEndPoint);
                }

                try
                {
                    var receiveTask = client.ReceiveAsync();
                    var delayTask = Task.Delay(timeout);
                    var done = await Task.WhenAny(receiveTask, delayTask);
                    if (done == receiveTask)
                    {
                        var result = receiveTask.Result;
                        var response = Encoding.UTF8.GetString(result.Buffer);
                        Console.WriteLine($"[Discovery] Yanıt alındı: {response} from {result.RemoteEndPoint}");
                        if (response.StartsWith(DiscoveryResponsePrefix, StringComparison.Ordinal))
                        {
                            var parts = response.Split(':');
                            int port;
                            if (parts.Length == 2 && int.TryParse(parts[1], out port))
                            {
                                var endPoint = new IPEndPoint(result.RemoteEndPoint.Address, port);
                                Console.WriteLine($"[Discovery] Sunucu bulundu: {endPoint}");
                                return endPoint;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("[Discovery] Timeout - sunucu bulunamadı");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Discovery] Hata: {ex.Message}");
                }
            }
            return null;
        }

        private IPEndPoint GetLocalSubnetBroadcast()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        var bytes = ip.GetAddressBytes();
                        bytes[3] = 255; // Son okteti 255 yap (broadcast)
                        var broadcastIp = new IPAddress(bytes);
                        return new IPEndPoint(broadcastIp, discoveryPort);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Discovery] Local subnet broadcast hesaplanamadı: {ex.Message}");
            }
            return null;
        }

        private void StartResponder()
        {
            try
            {
                listener = new UdpClient(new IPEndPoint(IPAddress.Any, discoveryPort));
                isServerResponderRunning = true;
                Console.WriteLine($"[Discovery] UDP listener başlatıldı (Port: {discoveryPort})");

                Task.Run(async () =>
                {
                    while (isServerResponderRunning)
                    {
                        try
                        {
                            var result = await listener.ReceiveAsync();
                            var text = Encoding.UTF8.GetString(result.Buffer);
                            Console.WriteLine($"[Discovery] İstek alındı: {text} from {result.RemoteEndPoint}");
                            if (text == DiscoveryRequest)
                            {
                                var response = DiscoveryResponsePrefix + tcpPort;
                                var bytes = Encoding.UTF8.GetBytes(response);
                                await listener.SendAsync(bytes, bytes.Length, result.RemoteEndPoint);
                                Console.WriteLine($"[Discovery] Yanıt gönderildi: {response} to {result.RemoteEndPoint}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Discovery] Listener hatası: {ex.Message}");
                            await Task.Delay(50);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Discovery] Listener başlatılamadı: {ex.Message}");
            }
        }

        public void Dispose()
        {
            isServerResponderRunning = false;
            try { if (listener != null) listener.Close(); } catch { }
            listener = null;
        }
    }
}