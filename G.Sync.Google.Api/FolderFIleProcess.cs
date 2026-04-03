using G.Sync.Entities.Interfaces;
using G.Sync.Google.Api.Interfaces;
using Google.Apis.Upload;
using File = Google.Apis.Drive.v3.Data.File;

namespace G.Sync.Google.Api
{
    public class FolderFileProcess : IFolderFileProcess
    {
        protected readonly IGoogleDriveService GoogleDriveService;

        public FolderFileProcess(IGoogleDriveService googleDriveService)
        {
            GoogleDriveService = googleDriveService;
        }

        public ISettingsEntity Settings { get; set; }

        public async Task<string> EnsureDriveFolderAsync(string relPath, string rootId)
        {
            Console.WriteLine($"O path é {relPath}");

            var parts = relPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

            var parentId = rootId;

            foreach (var part in parts)
            {
                Console.WriteLine($"Criando pasta com nome {part} com parent id sendo {parentId}");

                var result = await GoogleDriveService.ListFilesAsync(
                    $"mimeType='application/vnd.google-apps.folder' and name='{part}' and '{parentId}' in parents and trashed=false",
                    "files(id, name)"
                );

                if (result.Files.Count > 0)
                {
                    parentId = result.Files[0].Id;
                }
                else
                {
                    var folder = await GoogleDriveService.CreateFolderAsync(new File
                    {
                        Name = part,
                        MimeType = "application/vnd.google-apps.folder",
                        Parents = new[] { parentId }
                    }, "id");

                    parentId = folder.Id;
                }
            }

            return parentId;
        }

        public async Task<string> GetOrCreateRootFolderAsync()
        {
            var folderName = Settings.GoogleDriveFolderName;

            var result = await GoogleDriveService.ListFilesAsync(
                $"mimeType='application/vnd.google-apps.folder' and name='{folderName}' and trashed=false",
                "files(id)"
            );

            if (result.Files.Count > 0)
                return result.Files[0].Id;

            var folder = await GoogleDriveService.CreateFolderAsync(new File
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder"
            }, "id");

            return folder.Id;
        }

        public async Task<string> UploadFileAsync(string localRoot, string filePath, string driveRoot)
        {
            Console.WriteLine($"LocalRoot = {localRoot}, FilePath = {filePath}, DriveRoot = {driveRoot}");

            var relPath = Path.GetRelativePath(localRoot, filePath);

            Console.WriteLine($"Relative path = {relPath} ");

            var relDir = Path.GetDirectoryName(relPath);

            var vaultRootId = await GetOrCreateVaultRootFolderAsync(localRoot, driveRoot);

            string? parentId;

            if (string.IsNullOrWhiteSpace(relDir))
            {
                parentId = vaultRootId;
            }
            else
            {
                parentId = await EnsureDriveFolderAsync(relDir, vaultRootId);
            }

            var fileId = await FileExistsAsync(Path.GetFileName(relPath), parentId);

            if (!string.IsNullOrEmpty(fileId))
                return string.Empty;

            var upload = await GoogleDriveService.UploadFileAsync(parentId, filePath);

            if (upload.progress.Status != UploadStatus.Completed)
                throw new Exception(upload.progress.Exception?.Message);

            return upload.responseBody.Id;
        }

        public async Task<string> FileExistsAsync(string fileName, string parentId)
        {
            var result = await GoogleDriveService.ListFilesAsync(
                $"name='{fileName}' and '{parentId}' in parents and trashed=false",
                "files(id)"
            );

            return result.Files.Count > 0 ? result.Files[0].Id : string.Empty;
        }

        public async Task<string> UpdateFileAsync(string localRoot, string filePath, string driveRoot)
        {
            var relPath = Path.GetRelativePath(localRoot, filePath);
            var relDir = Path.GetDirectoryName(relPath);

            var vaultRootId = await GetOrCreateVaultRootFolderAsync(localRoot, driveRoot);

            string? parentId;

            if (string.IsNullOrWhiteSpace(relDir))
            {
                parentId = vaultRootId; 
            }
            else
            {
                parentId = await EnsureDriveFolderAsync(relDir, vaultRootId);
            }

            var id = await FileExistsAsync(Path.GetFileName(filePath), parentId);

            if (!string.IsNullOrWhiteSpace(id) && !IsFileLocked(filePath))
            {
                var update = await GoogleDriveService.UpdateFileAsync(id, filePath);

                if (update.progress.Status != UploadStatus.Completed)
                    throw new Exception(update.progress.Exception?.Message);

                return update.responseBody.Id;
            }
            else
            {
                await UploadFileAsync(localRoot, filePath, driveRoot);
            }

            return string.Empty;
        }

        private async Task<string> GetOrCreateVaultRootFolderAsync(string localVaultRoot, string driveRootId)
        {
            var vaultName = Path.GetFileName(localVaultRoot.TrimEnd(Path.DirectorySeparatorChar));

            var result = await GoogleDriveService.ListFilesAsync(
                $"mimeType='application/vnd.google-apps.folder' and name='{vaultName}' and '{driveRootId}' in parents and trashed=false",
                "files(id)"
            );

            if (result.Files.Count > 0)
                return result.Files[0].Id;

            var folder = await GoogleDriveService.CreateFolderAsync(new File
            {
                Name = vaultName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new[] { driveRootId } 
            }, "id");

            return folder.Id;
        }

        public async Task<string> DeleteFileAsync(string localRoot, string filePath, string driveRoot)
        {
            var relPath = Path.GetRelativePath(localRoot, filePath);
            var relDir = Path.GetDirectoryName(relPath);

            var vaultRootId = await GetOrCreateVaultRootFolderAsync(localRoot, driveRoot);

            var parentId = string.IsNullOrWhiteSpace(relDir)
                ? vaultRootId
                : await EnsureDriveFolderAsync(relDir, vaultRootId);

            var id = await FileExistsAsync(Path.GetFileName(filePath), parentId);

            if (!string.IsNullOrWhiteSpace(id))
            {
                await GoogleDriveService.DeleteFileAsync(id);
                return id;
            }

            Console.Write("Deleting file");

            return string.Empty;
        }

        public async Task<string> RenameFileAsync(string localRoot, string oldPath, string newPath, string driveRoot)
        {
            var relPath = Path.GetRelativePath(localRoot, oldPath);
            var relDir = Path.GetDirectoryName(relPath);

            var vaultRootId = await GetOrCreateVaultRootFolderAsync(localRoot, driveRoot);

            string? parentId;

            if (string.IsNullOrWhiteSpace(relDir))
            {
                parentId = vaultRootId;
            }
            else
            {
                parentId = await EnsureDriveFolderAsync(relDir, vaultRootId);
            }
            var id = await FileExistsAsync(Path.GetFileName(oldPath), parentId);

            if (!string.IsNullOrWhiteSpace(id))
            {
                var file = await GoogleDriveService.RenameFileAsync(id, Path.GetFileName(newPath));
                return file.Id;
            }
            else
            {
                await UploadFileAsync(localRoot, newPath, driveRoot);
            }

            return string.Empty;
        }

        public async Task DownloadAllFilesAsync(string driveRoot, string vaultPath)
        {
            async Task Download(string folderId, string localPath)
            {
                var result = await GoogleDriveService.ListFilesAsync(
                    $"'{folderId}' in parents and trashed=false",
                    "files(id, name, mimeType)"
                );

                foreach (var f in result.Files)
                {
                    var target = Path.Combine(localPath, f.Name);

                    if (f.MimeType == "application/vnd.google-apps.folder")
                    {
                        Directory.CreateDirectory(target);
                        await Download(f.Id, target);
                    }
                    else if (!System.IO.File.Exists(target))
                    {
                        await GoogleDriveService.DownloadFileAsync(target, f.Id);
                    }
                }
            }

            await Download(driveRoot, vaultPath);
        }

        private static bool IsFileLocked(string filePath)
        {
            try
            {
                using var stream = System.IO.File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                return false;
            }
            catch
            {
                return true;
            }
        }
    }
}