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

        public void DownloadAllFiles(string driveRoot) =>
            DownloadAllFilesInternal(driveRoot);
    }
}
