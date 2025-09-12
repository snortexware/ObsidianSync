using G.Sync.DataContracts;
using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using G.Sync.Google.Interfaces;
using G.Sync.Repository;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Upload;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using File = Google.Apis.Drive.v3.Data.File;
using FileS = System.IO.File;

namespace G.Sync.Google.Api
{
    public abstract class ApiFolderFileHelper
    {
        private readonly IGoogleDriveService _googleDriveService;
        private readonly ISettingsEntity _settings;

        public ApiFolderFileHelper(IGoogleDriveService _drive, ISettingsEntity settings)
        {
            _settings = settings;
            _googleDriveService = _drive;
        }

        protected string EnsureDriveFolderInternal(string relPath, string rootId)
        {
            var parts = relPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

            var parentId = rootId;

            foreach (var part in parts)
            {
                var query = $"mimeType='application/vnd.google-apps.folder' and name='{part}' and '{parentId}' in parents and trashed=false";
                var fields = "files(id, name)";

                var result = _googleDriveService.ListFiles(query, fields);

                string folderId;
                if (result.Files.Count > 0)
                {
                    folderId = result.Files[0].Id;
                }
                else
                {
                    var folderMeta = new File
                    {
                        Name = part,
                        MimeType = "application/vnd.google-apps.folder",
                        Parents = new[] { parentId }
                    };

                    var fieldsFolder = "id";

                    var createRequest = _googleDriveService.CreateFolder(folderMeta, fieldsFolder);
                    folderId = createRequest.Id;
                }

                parentId = folderId;
            }

            return parentId;
        }


        public string GetOrCreateRootFolderInternal()
        {
            var folderName = _settings.GoogleDriveFolderName;

            var query = $"mimeType = 'application/vnd.google-apps.folder' and name = '{folderName}' and trashed = false";
            var fields = "files(id)";

            var result = _googleDriveService.ListFiles(query, fields);

            if (result.Files.Count > 0)
                return result.Files[0].Id;

            var meta = new File
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder"
            };

            var fieldsFolder = "id";
            var folder = _googleDriveService.CreateFolder(meta, fieldsFolder);

            Console.WriteLine($"Pasta raiz criada com ID: {folder.Id}");
            return folder.Id;
        }

        public string UploadFileInternal(DriveService service, string localRoot, string filePath, string driveRoot)
        {
            var relPath = Path.GetRelativePath(localRoot, filePath);
            var relDir = Path.GetDirectoryName(relPath);
            var parentId = string.IsNullOrWhiteSpace(relDir) ? driveRoot : EnsureDriveFolderInternal(relDir, driveRoot);

            var upload = _googleDriveService.UploadFile(filePath, parentId);
            var uploadProgress = upload.progress;
            if (uploadProgress.Status != UploadStatus.Completed)
                throw new Exception($"Erro upload: {uploadProgress.Exception}");

            return upload.responseBody.Id;
        }

        public string FileExistsInternal(string fileName, string parentId)
        {
            var query = $"name = '{fileName}' and '{parentId}' in parents and trashed = false";
            var fields = "files(id)";
            var result = _googleDriveService.ListFiles(query, fields);
            return result.Files.Count > 0 ? result.Files[0].Id : string.Empty;
        }

        public string UpdateFileInternal(DriveService service, string localRoot, string filePath, string driveRoot)
        {
            var relPath = Path.GetRelativePath(localRoot, filePath);
            var relDir = Path.GetDirectoryName(relPath);
            var parentId = string.IsNullOrWhiteSpace(relDir) ? driveRoot : EnsureDriveFolderInternal(relDir, driveRoot);
            var fileName = Path.GetFileName(filePath);

            if (FileExistsInternal(fileName, parentId) is string id && !string.IsNullOrWhiteSpace(id))
            {
                using var stream = new FileStream(filePath, FileMode.Open);
                var update = _googleDriveService.UpdateFile(id, filePath);
                var updateProgress = update.progress;
                var status = updateProgress.Status;

                if (status != UploadStatus.Completed)
                    throw new Exception($"Erro update: {updateProgress.Exception}");

                return update.responseBody.Id;
            }

            return string.Empty;
        }

        public string DeleteFileInternal(string localRoot, string filePath, string driveRoot)
        {
            var relPath = Path.GetRelativePath(localRoot, filePath);
            var relDir = Path.GetDirectoryName(relPath);
            var parentId = string.IsNullOrWhiteSpace(relDir) ? driveRoot : EnsureDriveFolderInternal(relDir, driveRoot);
            var fileName = Path.GetFileName(filePath);

            if (FileExistsInternal(fileName, parentId) is string id && !string.IsNullOrWhiteSpace(id))
            {
                _googleDriveService.DeleteFile(id);
                return id;
            }
            return string.Empty;
        }

        public string RenameFileInternal(DriveService service, string localRoot, string oldPath, string newPath, string driveRoot)
        {
            var relOld = Path.GetRelativePath(localRoot, oldPath);
            var relDir = Path.GetDirectoryName(relOld);
            var parentId = string.IsNullOrWhiteSpace(relDir) ? driveRoot : EnsureDriveFolderInternal(relDir, driveRoot);
            var oldName = Path.GetFileName(oldPath);
            var newName = Path.GetFileName(newPath);

            if (FileExistsInternal(oldName, parentId) is string id && !string.IsNullOrWhiteSpace(id))
            {
                var meta = new File { Name = newName };
                var req = service.Files.Update(meta, id);
                req.Fields = "id";
                var file = req.Execute();
                return file.Id;
            }
            return string.Empty;
        }

        public void DownloadAllFilesInternal(DriveService service, string driveRoot)
        {
            var localRoot = _settings.GoogleDriveFolderName;
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            async Task DownloadFromFolder(string folderId, string localPath)
            {
                var req = service.Files.List();
                req.Q = $"'{folderId}' in parents and trashed = false";
                req.Fields = "files(id, name, mimeType, md5Checksum)";
                var result = await req.ExecuteAsync();
                foreach (var f in result.Files)
                {
                    var localTarget = Path.Combine(localPath, f.Name);
                    if (f.MimeType == "application/vnd.google-apps.folder")
                    {
                        Directory.CreateDirectory(localTarget);
                        await DownloadFromFolder(f.Id, localTarget);
                    }
                    else
                    {
                        if (!FileS.Exists(localTarget))
                        {
                            Console.WriteLine($"[{timestamp}] BAIXANDO {f.Name}");
                            using var stream = new FileStream(localTarget, FileMode.Create, FileAccess.Write);
                            var getReq = service.Files.Get(f.Id);
                            await getReq.DownloadAsync(stream);
                            Console.WriteLine($"[{timestamp}] SALVO {f.Name}");
                        }
                    }
                }
            }

            DownloadFromFolder(driveRoot, localRoot).GetAwaiter().GetResult();
        }
    }
}
