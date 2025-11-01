// Amaç: Uygulama akışını yönetir; otomatik keşif, sunucu başlatma/bağlanma ve mesajlaşma işlevleri.
using ChatApp.Networking;
using System;
using System.Net;
using System.Net.Sockets;
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
            ApplyFenerbahceTheme();
            textBoxUsername.Text = Environment.UserName;
            Shown += async (_, __) => await InitializeNetworkingAsync();
            FormClosing += MainForm_FormClosing;
        }

        private void ApplyFenerbahceTheme()
        {
            // Fenerbahçe renkleri
            var fenerbahceNavy = System.Drawing.Color.FromArgb(0, 51, 102); // Lacivert
            var fenerbahceYellow = System.Drawing.Color.FromArgb(255, 204, 0); // Sarı
            var fenerbahceDarkNavy = System.Drawing.Color.FromArgb(0, 26, 85); // Koyu lacivert
            var white = System.Drawing.Color.White;
            var darkGray = System.Drawing.Color.FromArgb(40, 40, 40);

            // Form arka planı - Lacivert
            this.BackColor = fenerbahceNavy;
            this.ForeColor = white;

            // RichTextBox - Beyaz arka plan, lacivert yazı
            richTextBoxChat.BackColor = white;
            richTextBoxChat.ForeColor = fenerbahceDarkNavy;
            richTextBoxChat.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);

            // Username TextBox - Sarı arka plan, lacivert yazı
            textBoxUsername.BackColor = fenerbahceYellow;
            textBoxUsername.ForeColor = fenerbahceDarkNavy;
            textBoxUsername.BorderStyle = BorderStyle.FixedSingle;
            textBoxUsername.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);

            // Message TextBox - Sarı arka plan, lacivert yazı
            textBoxMessage.BackColor = fenerbahceYellow;
            textBoxMessage.ForeColor = fenerbahceDarkNavy;
            textBoxMessage.BorderStyle = BorderStyle.FixedSingle;
            textBoxMessage.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);

            // Butonlar - Lacivert arka plan, sarı yazı
            buttonSend.BackColor = fenerbahceDarkNavy;
            buttonSend.ForeColor = fenerbahceYellow;
            buttonSend.FlatStyle = FlatStyle.Flat;
            buttonSend.FlatAppearance.BorderSize = 0;
            buttonSend.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            buttonSend.Cursor = Cursors.Hand;

            buttonHostToggle.BackColor = fenerbahceDarkNavy;
            buttonHostToggle.ForeColor = fenerbahceYellow;
            buttonHostToggle.FlatStyle = FlatStyle.Flat;
            buttonHostToggle.FlatAppearance.BorderSize = 0;
            buttonHostToggle.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            buttonHostToggle.Cursor = Cursors.Hand;

            buttonManualConnect.BackColor = fenerbahceDarkNavy;
            buttonManualConnect.ForeColor = fenerbahceYellow;
            buttonManualConnect.FlatStyle = FlatStyle.Flat;
            buttonManualConnect.FlatAppearance.BorderSize = 0;
            buttonManualConnect.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Bold);
            buttonManualConnect.Cursor = Cursors.Hand;

            // Buton hover efektleri
            buttonSend.MouseEnter += (s, e) => { buttonSend.BackColor = fenerbahceNavy; };
            buttonSend.MouseLeave += (s, e) => { buttonSend.BackColor = fenerbahceDarkNavy; };
            buttonHostToggle.MouseEnter += (s, e) => { buttonHostToggle.BackColor = fenerbahceNavy; };
            buttonHostToggle.MouseLeave += (s, e) => { buttonHostToggle.BackColor = fenerbahceDarkNavy; };
            buttonManualConnect.MouseEnter += (s, e) => { buttonManualConnect.BackColor = fenerbahceNavy; };
            buttonManualConnect.MouseLeave += (s, e) => { buttonManualConnect.BackColor = fenerbahceDarkNavy; };

            // Status Label - Sarı yazı, lacivert arka plan
            labelStatus.BackColor = fenerbahceDarkNavy;
            labelStatus.ForeColor = fenerbahceYellow;
            labelStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            labelStatus.Padding = new Padding(5);

            // Form başlığı
            this.Text = "💛💙 FenerChat 💙💛";
        }

        private async Task InitializeNetworkingAsync()
        {
            AppendChatLine("[Debug] Ağ başlatılıyor...");
            discoveryService = new DiscoveryService(DiscoveryPort, TcpPort);
            AppendChatLine($"[Debug] Discovery servisi başlatıldı (Port: {DiscoveryPort})");
            
            AppendChatLine("[Debug] Sunucu aranıyor...");
            var discovered = await discoveryService.TryDiscoverServerAsync(TimeSpan.FromSeconds(5));

            if (discovered == null)
            {
                AppendChatLine("[Debug] Sunucu bulunamadı, sunucu modu başlatılıyor...");
                StartHosting();
            }
            else
            {
                AppendChatLine($"[Debug] Sunucu bulundu: {discovered}");
                await ConnectToServerAsync(discovered);
            }
        }

        private void StartHosting()
        {
            try
            {
                AppendChatLine($"[Debug] TCP Sunucu başlatılıyor (Port: {TcpPort})");
                server = new TcpChatServer(IPAddress.Any, TcpPort);
                server.MessageReceived += Server_MessageReceived;
                server.ClientConnected += Server_ClientConnected;
                server.ClientDisconnected += Server_ClientDisconnected;
                server.Start();

                isHosting = true;
                labelStatus.Text = "Durum: Sunucu (dinlemede)";
                buttonHostToggle.Text = "Durdur";
                AppendChatLine("[Debug] TCP Sunucu başarıyla başlatıldı");
                ShowLocalIPs();
            }
            catch (Exception ex)
            {
                AppendChatLine($"[Hata] Sunucu başlatılamadı: {ex.Message}");
            }
        }

        private async Task ConnectToServerAsync(IPEndPoint serverEndPoint)
        {
            try
            {
                AppendChatLine($"[Debug] Sunucuya bağlanılıyor: {serverEndPoint}");
                client = new TcpChatClient(serverEndPoint.Address, serverEndPoint.Port, GetUsername);
                client.MessageReceived += Client_MessageReceived;

                var connected = await client.ConnectAsync(TimeSpan.FromSeconds(10));
                if (connected)
                {
                    isHosting = false;
                    labelStatus.Text = $"Durum: Bağlı ({serverEndPoint.Address})";
                    buttonHostToggle.Text = "Sunucu Ol";
                    AppendChatLine("[Debug] Sunucuya başarıyla bağlanıldı");
                    AppendChatLine("Sisteminize bağlanıldı. Sohbete başlayabilirsiniz.");
                }
                else
                {
                    AppendChatLine("[Hata] Sunucuya bağlanılamadı. Sunucu başlatılıyor...");
                    StartHosting();
                }
            }
            catch (Exception ex)
            {
                AppendChatLine($"[Hata] Bağlantı hatası: {ex.Message}");
                AppendChatLine("Sunucu başlatılıyor...");
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

            // Mesaj tipine göre renklendirme
            if (line.StartsWith("[Bilgi]"))
            {
                richTextBoxChat.SelectionColor = System.Drawing.Color.FromArgb(0, 51, 102); // Lacivert
                richTextBoxChat.SelectionFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            }
            else if (line.StartsWith("[Debug]"))
            {
                richTextBoxChat.SelectionColor = System.Drawing.Color.Gray;
                richTextBoxChat.SelectionFont = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic);
            }
            else if (line.StartsWith("[Hata]"))
            {
                richTextBoxChat.SelectionColor = System.Drawing.Color.Red;
                richTextBoxChat.SelectionFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            }
            else if (line.StartsWith("[Uyarı]"))
            {
                richTextBoxChat.SelectionColor = System.Drawing.Color.Orange;
                richTextBoxChat.SelectionFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            }
            else
            {
                // Normal mesajlar - Sarı renk ile kullanıcı adı vurgulama
                if (line.Contains(":"))
                {
                    var parts = line.Split(new[] { ':' }, 2);
                    if (parts.Length == 2)
                    {
                        richTextBoxChat.SelectionColor = System.Drawing.Color.FromArgb(255, 204, 0); // Sarı - Kullanıcı adı
                        richTextBoxChat.SelectionFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
                        richTextBoxChat.AppendText(parts[0] + ":");
                        
                        richTextBoxChat.SelectionColor = System.Drawing.Color.FromArgb(0, 26, 85); // Lacivert - Mesaj
                        richTextBoxChat.SelectionFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
                        richTextBoxChat.AppendText(parts[1]);
                        richTextBoxChat.AppendText(Environment.NewLine);
                        richTextBoxChat.ScrollToCaret();
                        return;
                    }
                }
                richTextBoxChat.SelectionColor = System.Drawing.Color.FromArgb(0, 26, 85); // Varsayılan lacivert
                richTextBoxChat.SelectionFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular);
            }

            richTextBoxChat.AppendText(line + Environment.NewLine);
            richTextBoxChat.ScrollToCaret();
        }

        private void ShowLocalIPs()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                AppendChatLine("[Bilgi] Bu bilgisayarın IP adresleri:");
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        AppendChatLine($"  - {ip}");
                    }
                }
            }
            catch (Exception ex)
            {
                AppendChatLine($"[Hata] IP adresleri alınamadı: {ex.Message}");
            }
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
                    AppendChatLine("[Debug] Sunucu aranıyor...");
                    var discovered = await discoveryService.TryDiscoverServerAsync(TimeSpan.FromSeconds(5));
                    if (discovered != null)
                    {
                        await ConnectToServerAsync(discovered);
                    }
                    else
                    {
                        labelStatus.Text = "Durum: Bağlı değil";
                        AppendChatLine("[Bilgi] Yakında sunucu bulunamadı.");
                        AppendChatLine("[Bilgi] Manuel bağlantı için: Sunucu IP'sini öğrenin ve 'Manuel Bağlan' butonuna basın.");
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

        private async void buttonManualConnect_Click(object sender, EventArgs e)
        {
            var input = Microsoft.VisualBasic.Interaction.InputBox(
                "Sunucu IP adresini girin:", 
                "Manuel Bağlantı", 
                "192.168.1.", 
                -1, -1);
            
            if (!string.IsNullOrWhiteSpace(input))
            {
                if (IPAddress.TryParse(input, out IPAddress ip))
                {
                    var endPoint = new IPEndPoint(ip, TcpPort);
                    AppendChatLine($"[Debug] Manuel bağlantı deneniyor: {endPoint}");
                    await ConnectToServerAsync(endPoint);
                }
                else
                {
                    AppendChatLine("[Hata] Geçersiz IP adresi formatı.");
                }
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

