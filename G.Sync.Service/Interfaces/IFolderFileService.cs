using G.Sync.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Service.Interfaces
{
    public interface IFolderFileService
    {
        void InjectDependencies(ISettingsEntity settings);
        string UploadFile(string localRoot, string filePath, string driveRoot);
        string UpdateFile(string localRoot, string filePath, string driveRoot);
        string DeleteFile(string localRoot, string filePath, string driveRoot);
        string RenameFile(string localRoot, string oldPath, string newPath, string driveRoot);
        void DownloadAllFiles(string driveRoot, string vaultPath);
    }

}
