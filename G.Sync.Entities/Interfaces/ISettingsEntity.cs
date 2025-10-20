using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Entities.Interfaces
{
    public interface ISettingsEntity
    {
        public long Id { get;}
        public string? Folder { get;}
        public string? DriveProjectName { get; }
        public string? GoogleDriveFolderName { get; }
        public SettingsEntity CreateSettings(string driveProjetoName, string googleDriveFolderName, string folder);
    }
}
