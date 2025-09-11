using G.Sync.DataContracts;
using G.Sync.Entities;
using G.Sync.Repository;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Upload;
using System;
using System.IO;
using System.Threading.Tasks;
using File = Google.Apis.Drive.v3.Data.File;
using FileS = System.IO.File;

namespace G.Sync.Google.Api
{
    public abstract class ApiFolderFileHelper
    {
        protected string EnsureDriveFolderInternal(DriveService service, string relPath, string rootId)
        {
            var parts = relPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

            var parentId = rootId;

            foreach (var part in parts)
            {
                var request = service.Files.List();
                request.Q = $"mimeType='application/vnd.google-apps.folder' and name='{part}' and '{parentId}' in parents and trashed=false";
                request.Spaces = "drive";
                request.Fields = "files(id, name)";
                var result = request.Execute();

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
                    var createRequest = service.Files.Create(folderMeta);
                    createRequest.Fields = "id";
                    folderId = createRequest.Execute().Id;
                }

                parentId = folderId;
            }

            return parentId;
        }


        public string GetOrCreateRootFolderInternal(DriveService service, SettingsEntity settings)
        {
            var folderName = settings.GoogleDriveFolderName ?? "obsidian-sync";

            var listReq = service.Files.List();
            listReq.Q = $"mimeType = 'application/vnd.google-apps.folder' and name = '{folderName}' and trashed = false";
            listReq.Fields = "files(id)";
            var result = listReq.Execute();

            if (result.Files.Count > 0)
                return result.Files[0].Id;

            var meta = new File
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder"
            };
            var createReq = service.Files.Create(meta);
            createReq.Fields = "id";
            var folder = createReq.Execute();
            Console.WriteLine($"Pasta raiz criada com ID: {folder.Id}");
            return folder.Id;
        }

        public string UploadFileInternal(DriveService service, string localRoot, string filePath, string driveRoot)
        {
            var relPath = Path.GetRelativePath(localRoot, filePath);
            var relDir = Path.GetDirectoryName(relPath);
            var parentId = string.IsNullOrWhiteSpace(relDir) ? driveRoot : EnsureDriveFolderInternal(service, relDir, driveRoot);

            var meta = new File { Name = Path.GetFileName(filePath), Parents = new[] { parentId } };
            using var stream = new FileStream(filePath, FileMode.Open);
            var req = service.Files.Create(meta, stream, "application/octet-stream");
            req.Fields = "id";
            var status = req.Upload();

            if (status.Status != UploadStatus.Completed)
                throw new Exception($"Erro upload: {status.Exception}");

            return req.ResponseBody.Id;
        }

        public string FileExistsInternal(DriveService service, string fileName, string parentId)
        {
            var listReq = service.Files.List();
            listReq.Q = $"name = '{fileName}' and '{parentId}' in parents and trashed = false";
            listReq.Fields = "files(id)";
            var result = listReq.Execute();
            return result.Files.Count > 0 ? result.Files[0].Id : string.Empty;
        }

        public string UpdateFileInternal(DriveService service, string localRoot, string filePath, string driveRoot)
        {
            var relPath = Path.GetRelativePath(localRoot, filePath);
            var relDir = Path.GetDirectoryName(relPath);
            var parentId = string.IsNullOrWhiteSpace(relDir) ? driveRoot : EnsureDriveFolderInternal(service, relDir, driveRoot);
            var fileName = Path.GetFileName(filePath);

            if (FileExistsInternal(service, fileName, parentId) is string id && !string.IsNullOrWhiteSpace(id))
            {
                using var stream = new FileStream(filePath, FileMode.Open);
                var req = service.Files.Update(null, id, stream, "application/octet-stream");
                req.Fields = "id";
                var status = req.Upload();

                if (status.Status != UploadStatus.Completed)
                    throw new Exception($"Erro update: {status.Exception}");
                return req.ResponseBody.Id;
            }
            return string.Empty;
        }

        public string DeleteFileInternal(DriveService service, string localRoot, string filePath, string driveRoot)
        {
            var relPath = Path.GetRelativePath(localRoot, filePath);
            var relDir = Path.GetDirectoryName(relPath);
            var parentId = string.IsNullOrWhiteSpace(relDir) ? driveRoot : EnsureDriveFolderInternal(service, relDir, driveRoot);
            var fileName = Path.GetFileName(filePath);

            if (FileExistsInternal(service, fileName, parentId) is string id && !string.IsNullOrWhiteSpace(id))
            {
                var req = service.Files.Delete(id);
                req.Execute();
                return id;
            }
            return string.Empty;
        }

        public string RenameFileInternal(DriveService service, string localRoot, string oldPath, string newPath, string driveRoot)
        {
            var relOld = Path.GetRelativePath(localRoot, oldPath);
            var relDir = Path.GetDirectoryName(relOld);
            var parentId = string.IsNullOrWhiteSpace(relDir) ? driveRoot : EnsureDriveFolderInternal(service, relDir, driveRoot);
            var oldName = Path.GetFileName(oldPath);
            var newName = Path.GetFileName(newPath);

            if (FileExistsInternal(service, oldName, parentId) is string id && !string.IsNullOrWhiteSpace(id))
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
            var settingsRepo = new SettingsRepository();
            var settings = settingsRepo.GetSettings();
            var localRoot = settings?.GoogleDriveFolderName ?? "obsidian-sync";
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
