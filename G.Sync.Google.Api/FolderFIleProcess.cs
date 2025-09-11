using G.Sync.DataContracts;
using G.Sync.Entities;
using Google.Apis.Drive.v3;
using System;
using System.IO;
using File = Google.Apis.Drive.v3.Data.File;

namespace G.Sync.Google.Api
{
    public abstract class FolderFileProcess : ApiFolderFileHelper
    {
        private readonly DriveService _service = ApiContext.Instance.Connection;

        public string EnsureDriveFolder(string relPath, string driveRoot) =>
            EnsureDriveFolderInternal(_service, relPath, driveRoot);

        public string GetOrCreateRootFolder(SettingsEntity settings) =>
            GetOrCreateRootFolderInternal(_service, settings);

        public string UploadFile(string localRoot, string filePath, string driveRoot) =>
            UploadFileInternal(_service, localRoot, filePath, driveRoot);

        public string UpdateFile(string localRoot, string filePath, string driveRoot) =>
            UpdateFileInternal(_service, localRoot, filePath, driveRoot);

        public string DeleteFile(string localRoot, string filePath, string driveRoot) =>
            DeleteFileInternal(_service, localRoot, filePath, driveRoot);

        public string RenameFile(string localRoot, string oldPath, string newPath, string driveRoot) =>
            RenameFileInternal(_service, localRoot, oldPath, newPath, driveRoot);

        public void DownloadAllFiles(string driveRoot) =>
            DownloadAllFilesInternal(_service, driveRoot);
    }
}
