using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using G.Sync.Utils;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace G.Sync.Common
{
    public class GSyncAppContext : IGSyncAppContext
    {
        [Inject]
        public ISettingsRepository SettingsRepository { get; set; }

        public ISettingsEntity GetAppSettings()
        {
            var settings = SettingsRepository.GetSettings();

            if (settings is not null)
                return settings;

            var appDataPath = GSyncAppHelper.GetAppDataFolder();

            var configFilePath = Path.Combine(appDataPath, GSyncAppConstants.ConfigurationFileName);

            if (!File.Exists(configFilePath))
                throw new Exception("Config not found.");

            var configurationJosn = File.ReadAllText(configFilePath);

            if (string.IsNullOrEmpty(configurationJosn))
                throw new Exception("Config not valid.");

            var configuration = JsonSerializer.Deserialize<ConfigurationDataObject>(configurationJosn);

            var newSettings = SettingsEntity.CreateSettings("ObsidianSync", configuration.GoogleDriveFolderName, appDataPath, configuration.IpAddress, configuration.Port);

            SettingsRepository.SaveSettings(newSettings);

            return newSettings;
        }

        private sealed class ConfigurationDataObject
        {
            public string IpAddress { get; set; }
            public int Port { get; set; }
            public string GoogleDriveFolderName { get; set; }
        }
    }
}
