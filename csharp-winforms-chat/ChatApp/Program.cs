// Amaç: WinForms giriş noktası (.NET Framework); ana formu çalıştırır.
using System;
using System.Windows.Forms;

namespace ChatApp
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}