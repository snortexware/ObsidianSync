using G.Sync.ConfigurationApp.Models;
using G.Sync.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Windows.UI.Popups;

namespace G.Sync.ConfigurationApp.ViewModels
{
    public partial class MainViewModel : INotifyPropertyChanged
    {
        private bool _isInstalling;
        public bool IsInstalling
        {
            get => _isInstalling;
            set { _isInstalling = value; OnPropertyChanged(); }
        }

        public async void StartService()
        {
            // Mostra o overlay
            IsInstalling = true;

            // Atualiza a UI
            await System.Threading.Tasks.Task.Delay(100); // pequena pausa para renderizar overlay

            // Dummy instalação (simula processo de 3 segundos)
            await System.Threading.Tasks.Task.Delay(3000);

            IsInstalling = false;

            // Mensagem final
            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = "Service",
                Content = "Service started successfully!",
                CloseButtonText = "Ok",
                XamlRoot = _window.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private readonly ConfigModel _config = new();

        public string IpAddress
        {
            get => _config.IpAddress;
            set { _config.IpAddress = value; OnPropertyChanged(); }
        }

        public int Port
        {
            get => _config.Port;
            set { _config.Port = value; OnPropertyChanged(); }
        }

        public string GoogleDriveFolderName
        {
            get => _config.GoogleDriveFolderName;
            set { _config.GoogleDriveFolderName = value; OnPropertyChanged(); }
        }

        public ICommand SaveConfig { get; }

        public MainViewModel()
        {
            SaveConfig = new RelayCommand(SaveConfiguration);
        }

        private Window _window;

        public void SetWindow(Window window)
        {
            _window = window;
        }

        public async void SaveConfiguration()
        {
            GSyncAppHelper.CreateJsonSettings(_config);

            var savedDialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = "Saved",
                Content = "Configuration saved successfully!",
                CloseButtonText = "Ok",
                XamlRoot = _window.Content.XamlRoot
            };

            await savedDialog.ShowAsync();

            var startDialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = "Service not running",
                Content = "Do you want to start the service now?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No, maybe later...",
                XamlRoot = _window.Content.XamlRoot

            };

            var result = await startDialog.ShowAsync();

            if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                StartService();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}