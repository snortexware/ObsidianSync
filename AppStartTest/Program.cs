using G.Sync.Entities;
using G.Sync.External.IO;
using G.Sync.External.IO.Quartz;
using G.Sync.Google.Api;
using G.Sync.Repository;


var rawVaults = VaultManager.GetAllActiveVaults();

var vaultRepo = new VaultsRepository();

foreach (var vault in rawVaults.Vaults)
{

    var vaultInfo = vault.Value;
    var vaultEntity = 
        new VaultsEntity(vaultInfo.Path, vaultInfo.Timestamp, vaultInfo.Open, vault.Key);

    vaultRepo.CreateVault(vaultEntity);
}

var vaults = vaultRepo.GetVaults();

var settings = new SettingsEntity();

var newSettings = settings.CreateSettings("ObsidianSync", "C:\\obsidian-sync", "obsidian-sync");

var settingsRepo = new SettingsRepository();

settingsRepo.SaveSettings(newSettings);

var json = File.ReadAllText("C:\\Users\\lucas\\OneDrive\\Documentos\\client_secret.json");

ApiContext.SetJson(json);

var queueStarter = new QueueStarter();
await queueStarter.StartQueueProcessor();

FileWatcherEventsController.Instance.StartWatching(vaults, newSettings);

Thread.Sleep(100000000);


