using G.Sync.DataContracts;
using G.Sync.Entities;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Upload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Dapper.SqlMapper;
using File = Google.Apis.Drive.v3.Data.File;

namespace G.Sync.Google.Api
{
    public abstract class ApiFolderFileHelper
    {
        public string EnsureDriverFolderInternal(DriveService service, ApiPathDto dto)
        {
            var listReq = service.Files.List();
            listReq.Q = $"mimeType = 'application/vnd.google-apps.folder' and name = '{dto.PathPart}' and '{dto.ParentId}' in parents and trashed = false";
            listReq.Fields = "files(id)";
            var found = listReq.Execute();
            if (found.Files.Count > 0) return found.Files[0].Id;

            var meta = new File
            {
                Name = dto.PathPart,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new[] { dto.ParentId }
            };

            var createReq = service.Files.Create(meta);
            createReq.Fields = "id";
            var folder = createReq.Execute();

            return folder.Id;
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
            else
            {
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
        }

        public string UploadFileInternal(DriveService service, ApiPathDto dto)
        {
            var relPath = Path.GetRelativePath(dto.BaseLocal, dto.FilePath);
            var relDir = Path.GetDirectoryName(relPath);
            var parentId = string.IsNullOrWhiteSpace(relDir) ? dto.RootId : EnsureDriverFolderInternal(service, new ApiPathDto { RelPath = relDir, RootId = dto.RootId, BaseLocal = dto.BaseLocal });

            var meta = new File { Name = Path.GetFileName(dto.FilePath), Parents = new[] { parentId } };
            using var stream = new FileStream(dto.FilePath, FileMode.Open);
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
            if (result.Files.Count > 0)
                return result.Files[0].Id;
            else
                return string.Empty;
        }

        public string UpdateFileInternal(DriveService _service, string localRoot, string driveRoot)
        {
            var relPath = Path.GetRelativePath(localRoot, localRoot);
            var relDir = Path.GetDirectoryName(relPath);

            var parentId = string.IsNullOrWhiteSpace(relDir) ? driveRoot : EnsureDriverFolderInternal(_service, new ApiPathDto { RelPath = relDir, RootId = driveRoot, BaseLocal = localRoot });

            var fileName = Path.GetFileName(localRoot);

            if (FileExistsInternal(_service, fileName, parentId) is string id && !string.IsNullOrWhiteSpace(id))
            {
                using var stream = new FileStream(localRoot, FileMode.Open);
                var req = _service.Files.Update(null, id, stream, "application/octet-stream");
                req.Fields = "id";
                var status = req.Upload();

                if (status.Status != UploadStatus.Completed)
                    throw new Exception($"Erro update: {status.Exception}");
                return req.ResponseBody.Id;
            }

            return string.Empty;
        }
    }
}
