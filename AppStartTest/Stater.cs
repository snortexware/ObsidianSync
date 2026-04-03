using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using G.Sync.External.IO;
using G.Sync.External.IO.Quartz;
using G.Sync.Google.Api;
using G.Sync.Repository;
using Ninject;
using G.Sync.IoC;
using G.Sync.Utils;

namespace AppStartTest
{
    public class Stater : IStarter
    {
        [Inject]
        public IVaultsRepository VaultRepo { get; set; }

        [Inject]
        public ISettingsRepository SettingsRepo { get; set; }

        [Inject]
        public IQueueStarter QueueStarter { get; set; }

        [Inject]
        public IFileWatcher FileWatcher { get; set; }

        [Inject]
        public IDatabaseInitializer DatabaseInitializer { get; set; }

        public async Task StartAsync()
        {
            try
            {

                DatabaseInitializer.Initialize();

                var rawVaults = VaultManager.GetAllActiveVaults();

                foreach (var vault in rawVaults.Vaults)
                {
                    var vaultEntity = new VaultsEntity(
                        vault.Value.Path,
                        vault.Value.Timestamp,
                        vault.Value.Open,
                        vault.Key
                    );

                    VaultRepo.CreateVault(vaultEntity);
                }

                var vaults = VaultRepo.GetVaults();

                var settings = SettingsRepo.GetSettings();

                if (settings is null)
                {
                    settings = new SettingsEntity().CreateSettings(
                        "ObsidianSync",
                        GetDefaultLocalFolder(),
                        GetDefaultDrivePath()
                    );

                    SettingsRepo.SaveSettings(settings);
                }

                var json = File.ReadAllText(GetClientSecretPath());

                await QueueStarter.StartQueueProcessor();

                FileWatcher.StartWatching(vaults, settings);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.Message);
                throw;
            }

            await Task.Delay(Timeout.Infinite);
        }

        private string GetDefaultDrivePath() => Path.Combine(GetAppDataFolder(), "ObsidianSync");
        private string GetDefaultLocalFolder() => "obsidian-sync";

        private string GetAppDataFolder()
        {
            string appName = "ObsidianSync";
            return Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName),
                PlatformID.Unix => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $".{appName}"),
                _ => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName)
            };
        }

        private string GetClientSecretPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "client_secret.json");
        }
    }
}