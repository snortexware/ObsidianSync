using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace G.Sync.External.IO
{
    public static class VaultManager
    {
        public static ObsidianVaultConfigDto GetAllActiveVaults()
        {
            var obsidianVaultsJsonPath = 
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "obsidian", "obsidian.json");

            var file = File.ReadAllText(obsidianVaultsJsonPath);

            var vaulInfo = JsonSerializer.Deserialize<ObsidianVaultConfigDto>(file);

            return vaulInfo is null ? throw new InvalidDataException("Could not deserialize Obsidian vault config.") : vaulInfo;
        }

    }
}
