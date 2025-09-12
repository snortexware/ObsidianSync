using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Entities.Interfaces
{
    public interface ISettingsEntity
    {
        public int Id { get;}
        public string? Folder { get;}
        public string? DriveProjectName { get; }
        public string? GoogleDriveFolderName { get; }
        public ISettingsEntity CreateSettings(string driveProjetoName, string googleDriveFolderName, string folder);
    }
}
