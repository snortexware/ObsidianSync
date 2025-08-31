using G.Sync.DataContracts;
using G.Sync.Entities;
using Google.Apis.Drive.v3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    }
}
