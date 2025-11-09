using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace G.Sync.External.IO
{
    public class ObsidianVaultConfigDto
    {
        [JsonPropertyName("vaults")]
        public Dictionary<string, VaultInfoDto> Vaults { get; set; } = new();
    }
    
    public class VaultInfoDto
    {
        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("ts")]
        public long Timestamp { get; set; }

        [JsonPropertyName("open")]
        public bool Open { get; set; }

    }
}
