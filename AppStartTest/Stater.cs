using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using G.Sync.External.IO;
using G.Sync.External.IO.Quartz;
using G.Sync.Google.Api;
using G.Sync.Repository;
using Ninject;
using G.Sync.IoC;
using G.Sync.Utils;
using G.Sync.Common;

namespace AppStartTest
{
    public class Stater : IStarter
    {
        [Inject]
        public IVaultsRepository VaultRepo { get; set; }


        [Inject]
        public IQueueStarter QueueStarter { get; set; }

        [Inject]
        public IFileWatcher FileWatcher { get; set; }

        [Inject]
        public IDatabaseInitializer DatabaseInitializer { get; set; }

        [Inject]
        public IGSyncAppContext GSyncAppContext { get; set; }

        private ISettingsEntity _settings { get; set; }

        public ISettingsEntity Settings
        {
            get
            {
                if (_settings is not null)
                    return _settings;

                return GSyncAppContext.GetAppSettings();
            }
        }

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

                var json = File.ReadAllText(GetClientSecretPath());

                await QueueStarter.StartQueueProcessor();

                if (Settings is null)
                    throw new Exception("Settings are empty");

                FileWatcher.StartWatching(vaults, Settings);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.Message);
                throw;
            }

            await Task.Delay(Timeout.Infinite);
        }


        private string GetClientSecretPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "client_secret.json");
        }
    }
}