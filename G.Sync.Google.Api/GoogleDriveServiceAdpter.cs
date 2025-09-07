using G.Sync.Google.Interfaces;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = Google.Apis.Drive.v3.Data.File;
namespace G.Sync.Google.Api
{
    public class GoogleDriveServiceAdapter : IGoogleDriveService
    {
        private readonly DriveService _service;

        public GoogleDriveServiceAdapter(DriveService service)
        {
            _service = service;
        }

        public FileList ListFiles(string query, string fields, string spaces = "drive")
        {
            var request = _service.Files.List();
            request.Q = query;
            request.Fields = fields;
            request.Spaces = spaces;
            return request.Execute();
        }

        public FileList ListFilesAsync(string query, string fields, string spaces = "drive")
        {
            var request = _service.Files.List();
            request.Q = query;
            request.Fields = fields;
            request.Spaces = spaces;
            return request.ExecuteAsync().Result;
        }

        public File CreateFolder(string name, string parentId)
        {
            var folderMeta = new File
            {
                Name = name,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new[] { parentId }
            };

            var createRequest = _service.Files.Create(folderMeta);
            createRequest.Fields = "id, name";
            return createRequest.Execute();
        }

        public File UploadFile(string parentId, string localPath)
        {
            var meta = new File { Name = Path.GetFileName(localPath), Parents = new[] { parentId } };
            using var stream = new FileStream(localPath, FileMode.Open);
            var req = _service.Files.Create(meta, stream, "application/octet-stream");
            req.Fields = "id, name";
            req.Upload();
            return req.ResponseBody;
        }

        public File UpdateFile(string id, string localPath)
        {
            using var stream = new FileStream(localPath, FileMode.Open);
            var req = _service.Files.Update(null, id, stream, "application/octet-stream");
            req.Fields = "id, name";
            req.Upload();
            return req.ResponseBody;
        }

        public void DeleteFile(string id)
        {
            _service.Files.Delete(id).Execute();
        }

        public File RenameFile(string id, string newName)
        {
            var meta = new File { Name = newName };
            var req = _service.Files.Update(meta, id);
            req.Fields = "id, name";
            return req.Execute();
        }
    }

}
