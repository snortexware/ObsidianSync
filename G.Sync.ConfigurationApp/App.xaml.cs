using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace G.Sync.ConfigurationApp
{
    public partial class App : Application
    {
        // Torna a janela principal acessível globalmente
        public static Window MainAppWindow { get; private set; }

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MainAppWindow = new MainWindow();
            MainAppWindow.Activate();
        }
    }
}