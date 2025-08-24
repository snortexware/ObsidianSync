using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.DataContracts
{
    /// <summary>
    /// Data transfer object for application settings.
    /// </summary>
    public class SettingsDto
    {
        public string Folder { get; set; } = string.Empty;
    }
}
