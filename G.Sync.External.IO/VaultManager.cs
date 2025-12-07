using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace G.Sync.External.IO
{
    public static class VaultManager
    {
        public static ObsidianVaultConfigDto GetAllActiveVaults()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return ReadWindowsJson();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return ReadLinuxFlatpakJson();

            throw new PlatformNotSupportedException("Unsupported OS");
        }

        // -----------------------------------------------------------
        // WINDOWS
        // -----------------------------------------------------------
        private static ObsidianVaultConfigDto ReadWindowsJson()
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "obsidian",
                "obsidian.json");

            if (!File.Exists(path))
                throw new FileNotFoundException($"Obsidian config not found: {path}");

            var json = File.ReadAllText(path);

            return JsonSerializer.Deserialize<ObsidianVaultConfigDto>(json)
                   ?? throw new InvalidDataException("Invalid Obsidian JSON.");
        }

        // -----------------------------------------------------------
        // LINUX (Flatpak only)
        // -----------------------------------------------------------
        private static ObsidianVaultConfigDto ReadLinuxFlatpakJson()
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            string flatpakConfig = Path.Combine(
                home,
                ".var", "app", "md.obsidian.Obsidian",
                "config", "obsidian", "obsidian.json"
            );

            if (!File.Exists(flatpakConfig))
            {
                throw new FileNotFoundException(
                    "Obsidian was NOT installed via Flatpak. " +
                    "Install the Flatpak version or add support for native AppImage."
                );
            }

            string json = File.ReadAllText(flatpakConfig);

            return JsonSerializer.Deserialize<ObsidianVaultConfigDto>(json)
                   ?? throw new InvalidDataException("Invalid obsidian.json.");
        }
    }
}
