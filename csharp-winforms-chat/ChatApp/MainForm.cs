// Amaç: Uygulama akışını yönetir; otomatik keşif, sunucu başlatma/bağlanma ve mesajlaşma işlevleri.
using ChatApp.Networking;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatApp
{
    public partial class MainForm : Form
    {
        private const int DiscoveryPort = 49876;
        private const int TcpPort = 55555;

        private DiscoveryService discoveryService;
        private TcpChatServer server;
        private TcpChatClient client;
        private volatile bool isHosting;

        public MainForm()
        {
            InitializeComponent();
            textBoxUsername.Text = Environment.UserName;
            Shown += async (_, __) => await InitializeNetworkingAsync();
            FormClosing += MainForm_FormClosing;
        }

        private async Task InitializeNetworkingAsync()
        {
            discoveryService = new DiscoveryService(DiscoveryPort, TcpPort);
            var discovered = await discoveryService.TryDiscoverServerAsync(TimeSpan.FromSeconds(2));

            if (discovered == null)
            {
                StartHosting();
            }
            else
            {
                await ConnectToServerAsync(discovered);
            }
        }

        private void StartHosting()
        {
            server = new TcpChatServer(IPAddress.Any, TcpPort);
            server.MessageReceived += Server_MessageReceived;
            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;
            server.Start();

            isHosting = true;
            labelStatus.Text = "Durum: Sunucu (dinlemede)";
            buttonHostToggle.Text = "Durdur";
        }

        private async Task ConnectToServerAsync(IPEndPoint serverEndPoint)
        {
            client = new TcpChatClient(serverEndPoint.Address, serverEndPoint.Port, GetUsername);
            client.MessageReceived += Client_MessageReceived;

            var connected = await client.ConnectAsync(TimeSpan.FromSeconds(3));
            if (connected)
            {
                isHosting = false;
                labelStatus.Text = $"Durum: Bağlı ({serverEndPoint.Address})";
                buttonHostToggle.Text = "Sunucu Ol";
                AppendChatLine("Sisteminize bağlanıldı. Sohbete başlayabilirsiniz.");
            }
            else
            {
                AppendChatLine("Sunucuya bağlanılamadı. Sunucu başlatılıyor...");
                StartHosting();
            }
        }

        private string GetUsername()
        {
            var name = textBoxUsername.Text.Trim();
            return string.IsNullOrWhiteSpace(name) ? "Kullanıcı" : name;
        }

        private void Server_ClientDisconnected(object sender, string e)
        {
            AppendChatLine("[Bilgi] " + e + " ayrıldı.");
        }

        private void Server_ClientConnected(object sender, string e)
        {
            AppendChatLine("[Bilgi] " + e + " bağlandı.");
        }

        private void Server_MessageReceived(object sender, string e)
        {
            AppendChatLine(e);
        }

        private void Client_MessageReceived(object sender, string e)
        {
            AppendChatLine(e);
        }

        private void AppendChatLine(string line)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(AppendChatLine), line);
                return;
            }
            richTextBoxChat.AppendText(line + Environment.NewLine);
        }

        private async void buttonSend_Click(object sender, EventArgs e)
        {
            var message = textBoxMessage.Text.Trim();
            if (string.IsNullOrWhiteSpace(message))
                return;

            textBoxMessage.Clear();

            if (isHosting && server != null)
            {
                var payload = $"{GetUsername()}: {message}";
                server.Broadcast(payload);
                AppendChatLine(payload);
            }
            else if (client != null && client.IsConnected)
            {
                await client.SendAsync($"{GetUsername()}: {message}");
            }
            else
            {
                AppendChatLine("[Uyarı] Bağlı değil.");
            }
        }

        private async void textBoxMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                buttonSend_Click(sender, EventArgs.Empty);
            }
        }

        private async void buttonHostToggle_Click(object sender, EventArgs e)
        {
            if (isHosting)
            {
                StopHosting();
                if (discoveryService != null)
                {
                    var discovered = await discoveryService.TryDiscoverServerAsync(TimeSpan.FromSeconds(2));
                    if (discovered != null)
                    {
                        await ConnectToServerAsync(discovered);
                    }
                    else
                    {
                        labelStatus.Text = "Durum: Bağlı değil";
                        AppendChatLine("[Bilgi] Yakında sunucu bulunamadı.");
                    }
                }
            }
            else
            {
                DisconnectClient();
                StartHosting();
            }
        }

        private void StopHosting()
        {
            if (server != null) server.Stop();
            server = null;
            isHosting = false;
            labelStatus.Text = "Durum: Bağlı değil";
            buttonHostToggle.Text = "Sunucu Ol";
        }

        private void DisconnectClient()
        {
            if (client != null)
            {
                client.Dispose();
                client = null;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopHosting();
            DisconnectClient();
            if (discoveryService != null) discoveryService.Dispose();
        }
    }
}

