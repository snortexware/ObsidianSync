using G.Sync.Google.Interfaces;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Upload;
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

        public File CreateFolder(File folderMeta, string fields)
        {   
            var createRequest = _service.Files.Create(folderMeta);
            createRequest.Fields = fields;
            return createRequest.Execute();
        }

        public (File responseBody, IUploadProgress progress) UploadFile(string parentId, string localPath)
        {
            var meta = new File { Name = Path.GetFileName(localPath), Parents = new[] { parentId } };
            using var stream = new FileStream(localPath, FileMode.Open);
            var req = _service.Files.Create(meta, stream, "application/octet-stream");
            req.Fields = "id, name";
            var progres = req.Upload();
            return (req.ResponseBody, progres);
        }

        public (File responseBody, IUploadProgress progress) UpdateFile(string id, string localPath)
        {
            using var stream = new FileStream(localPath, FileMode.Open);
            var req = _service.Files.Update(null, id, stream, "application/octet-stream");
            req.Fields = "id, name";
            var progress = req.Upload();
            return (req.ResponseBody, progress);
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
