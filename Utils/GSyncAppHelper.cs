using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace G.Sync.Utils
{
    public static class GSyncAppHelper
    {
        public static string GetAppDataFolder()
        {
            return Environment.OSVersion.Platform switch
            {
                PlatformID.Win32NT => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), GSyncAppConstants.AppDataName),
                PlatformID.Unix => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $".{GSyncAppConstants.AppDataName}"),
                _ => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), GSyncAppConstants.AppDataName)
            };
        }

        public static void CreateJsonSettings(object settings)
        {
            var json = JsonSerializer.Serialize(settings);

            var appDataPath = GetAppDataFolder();

            if (!Path.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            var configPath = Path.Combine(appDataPath, GSyncAppConstants.ConfigurationFileName);

            File.WriteAllText(configPath, json);
        }
    }
}
