using G.Sync.Entities.Interfaces;
using G.Sync.Google.Api;
using G.Sync.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Service
{
    public class FolderFileService : IFolderFileService
    {
        private readonly FolderFileProcess _process = new FolderFileProcess();
        public void InjectDependencies(ISettingsEntity settings) => _process.InjectDepedencies(settings);
        public string UploadFile(string localRoot, string filePath, string driveRoot) => _process.UploadFile(localRoot, filePath, driveRoot);
        public string UpdateFile(string localRoot, string filePath, string driveRoot) => _process.UpdateFile(localRoot, filePath, driveRoot);
        public string DeleteFile(string localRoot, string filePath, string driveRoot) => _process.DeleteFile(localRoot, filePath, driveRoot);
        public string RenameFile(string localRoot, string oldPath, string newPath, string driveRoot) => _process.RenameFile(localRoot, oldPath, newPath, driveRoot);
        public void DownloadAllFiles(string driveRoot) => _process.DownloadAllFiles(driveRoot);
    }

}
