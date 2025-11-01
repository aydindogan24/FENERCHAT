// Amaç: Ana form bileşenlerini tanımlar ve olay bağlarını yapar (.NET Framework uyumlu).
using System.Windows.Forms;

namespace ChatApp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox textBoxUsername;
        private System.Windows.Forms.RichTextBox richTextBoxChat;
        private System.Windows.Forms.TextBox textBoxMessage;
        private System.Windows.Forms.Button buttonSend;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Button buttonHostToggle;
        private System.Windows.Forms.Button buttonManualConnect;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.textBoxUsername = new System.Windows.Forms.TextBox();
            this.richTextBoxChat = new System.Windows.Forms.RichTextBox();
            this.textBoxMessage = new System.Windows.Forms.TextBox();
            this.buttonSend = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.buttonHostToggle = new System.Windows.Forms.Button();
            this.buttonManualConnect = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxUsername
            // 
            this.textBoxUsername.Location = new System.Drawing.Point(12, 12);
            this.textBoxUsername.Name = "textBoxUsername";
            this.textBoxUsername.Size = new System.Drawing.Size(220, 20);
            this.textBoxUsername.TabIndex = 0;
            this.textBoxUsername.Text = "";
            // 
            // richTextBoxChat
            // 
            this.richTextBoxChat.Location = new System.Drawing.Point(12, 41);
            this.richTextBoxChat.Name = "richTextBoxChat";
            this.richTextBoxChat.ReadOnly = true;
            this.richTextBoxChat.Size = new System.Drawing.Size(560, 300);
            this.richTextBoxChat.TabIndex = 1;
            this.richTextBoxChat.Text = "";
            // 
            // textBoxMessage
            // 
            this.textBoxMessage.Location = new System.Drawing.Point(12, 347);
            this.textBoxMessage.Name = "textBoxMessage";
            this.textBoxMessage.Size = new System.Drawing.Size(479, 20);
            this.textBoxMessage.TabIndex = 2;
            this.textBoxMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBoxMessage_KeyDown);
            // 
            // buttonSend
            // 
            this.buttonSend.Location = new System.Drawing.Point(497, 345);
            this.buttonSend.Name = "buttonSend";
            this.buttonSend.Size = new System.Drawing.Size(75, 23);
            this.buttonSend.TabIndex = 3;
            this.buttonSend.Text = "Gönder";
            this.buttonSend.UseVisualStyleBackColor = true;
            this.buttonSend.Click += new System.EventHandler(this.buttonSend_Click);
            // 
            // labelStatus
            // 
            this.labelStatus.Location = new System.Drawing.Point(238, 12);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(253, 23);
            this.labelStatus.TabIndex = 4;
            this.labelStatus.Text = "Durum: Başlatılıyor...";
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonHostToggle
            // 
            this.buttonHostToggle.Location = new System.Drawing.Point(497, 11);
            this.buttonHostToggle.Name = "buttonHostToggle";
            this.buttonHostToggle.Size = new System.Drawing.Size(75, 23);
            this.buttonHostToggle.TabIndex = 5;
            this.buttonHostToggle.Text = "Durdur";
            this.buttonHostToggle.UseVisualStyleBackColor = true;
            this.buttonHostToggle.Click += new System.EventHandler(this.buttonHostToggle_Click);
            // 
            // buttonManualConnect
            // 
            this.buttonManualConnect.Location = new System.Drawing.Point(416, 11);
            this.buttonManualConnect.Name = "buttonManualConnect";
            this.buttonManualConnect.Size = new System.Drawing.Size(75, 23);
            this.buttonManualConnect.TabIndex = 6;
            this.buttonManualConnect.Text = "Manuel Bağlan";
            this.buttonManualConnect.UseVisualStyleBackColor = true;
            this.buttonManualConnect.Click += new System.EventHandler(this.buttonManualConnect_Click);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(584, 381);
            this.Controls.Add(this.buttonManualConnect);
            this.Controls.Add(this.buttonHostToggle);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonSend);
            this.Controls.Add(this.textBoxMessage);
            this.Controls.Add(this.richTextBoxChat);
            this.Controls.Add(this.textBoxUsername);
            this.Name = "MainForm";
            this.Text = "WinForms Chat";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}