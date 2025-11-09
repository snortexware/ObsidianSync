using G.Sync.DataContracts;
using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using Google.Apis.Drive.v3;
using System;
using System.IO;
using File = Google.Apis.Drive.v3.Data.File;

namespace G.Sync.Google.Api
{
    public class FolderFileProcess : ApiFolderFileHelper
    {
        public void InjectDepedencies(ISettingsEntity settingsRepository)
        {
            var api = ApiContext.Connection;
            var dri = new GoogleDriveServiceAdapter(api);

            if (settingsRepository == null)
            {
                throw new Exception("Settings not found in database.");
            }

            Inject(dri, settingsRepository);
        }

        public string EnsureDriveFolder(string relPath, string rootId) =>
            EnsureDriveFolderInternal(relPath, rootId);

        public string GetOrCreateRootFolder() =>
            GetOrCreateRootFolderInternal();

        public string UploadFile(string localRoot, string filePath, string driveRoot) =>
            UploadFileInternal(localRoot, filePath, driveRoot);

        public string UpdateFile(string localRoot, string filePath, string driveRoot) =>
            UpdateFileInternal(localRoot, filePath, driveRoot);

        public string DeleteFile(string localRoot, string filePath, string driveRoot) =>
            DeleteFileInternal(localRoot, filePath, driveRoot);

        public string RenameFile(string localRoot, string oldPath, string newPath, string driveRoot) =>
            RenameFileInternal(localRoot, oldPath, newPath, driveRoot);

        public async void DownloadAllFiles(string driveRoot, string vaultPath) =>
            DownloadAllFilesInternal(driveRoot, vaultPath);
    }
}
