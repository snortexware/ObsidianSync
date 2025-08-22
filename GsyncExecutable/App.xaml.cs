using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Application = System.Windows.Application;
using System.Windows.Forms;

namespace GsyncExecutable
{
    public partial class App : Application
    {
        private NotifyIcon _trayIcon;
        private IHost _webHost;

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            // 1️⃣ Start ASP.NET Core host in-process
            _webHost = new WebHostBuilder()
    .UseKestrel()
    .UseUrls("http://localhost:5000")
    .UseStartup<YourAspNetStartup>()
    .Build();

            await _webHost.StartAsync();

            // 2️⃣ Create tray icon
            _trayIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon("app.ico"),
                Visible = true,
                Text = "ASP.NET Core + Tray"
            };

            // 3️⃣ Context menu
            var menu = new ContextMenu();
            menu.MenuItems.Add("Abrir WebApp", (s, ev) =>
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "http://localhost:5000",
                    UseShellExecute = true
                });
            });
            menu.MenuItems.Add("Sair", (s, ev) =>
            {
                ExitApp();
            });

            _trayIcon.ContextMenu = menu;
        }

        private async void ExitApp()
        {
            try
            {
                if (_webHost != null)
                {
                    await _webHost.StopAsync();
                    _webHost.Dispose();
                }
            }
            catch { }

            _trayIcon.Visible = false;
            Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            ExitApp();
        }
    }
}
