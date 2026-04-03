using G.Sync.Google.Api.Interfaces;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Upload;
using File = Google.Apis.Drive.v3.Data.File;

namespace G.Sync.Google.Api
{
    public class GoogleDriveServiceAdapter : IGoogleDriveService
    {
        private readonly IGoogleDriveContext _context;

        private DriveService? _service;

        public GoogleDriveServiceAdapter(IGoogleDriveContext context)
        {
            _context = context;
        }

        private async Task<DriveService> GetServiceAsync()
        {
            if (_service != null)
                return _service;

            _service = await _context.GetConnectionAsync();
            return _service;
        }

        public async Task<FileList> ListFilesAsync(string query, string fields, string spaces = "drive")
        {
            var service = await GetServiceAsync();

            var request = service.Files.List();
            request.Q = query;
            request.Fields = fields;
            request.Spaces = spaces;

            return await request.ExecuteAsync();
        }

        public async Task<File> CreateFolderAsync(File folderMeta, string fields)
        {
            var service = await GetServiceAsync();

            var request = service.Files.Create(folderMeta);
            request.Fields = fields;

            return await request.ExecuteAsync();
        }

        public async Task<(File, IUploadProgress)> UploadFileAsync(string parentId, string localPath)
        {
            var service = await GetServiceAsync();

            var meta = new File
            {
                Name = Path.GetFileName(localPath),
                Parents = new[] { parentId }
            };

            using var stream = new FileStream(localPath, FileMode.Open, FileAccess.Read);

            var request = service.Files.Create(meta, stream, "application/octet-stream");
            request.Fields = "id, name";

            var progress = await request.UploadAsync();

            return (request.ResponseBody, progress);
        }

        public async Task<(File, IUploadProgress)> UpdateFileAsync(string id, string localPath)
        {
            var service = await GetServiceAsync();

            using var stream = new FileStream(localPath, FileMode.Open, FileAccess.Read);

            var request = service.Files.Update(null, id, stream, "application/octet-stream");
            request.Fields = "id, name";

            var progress = await request.UploadAsync();

            return (request.ResponseBody, progress);
        }

        public async Task DeleteFileAsync(string id)
        {
            var service = await GetServiceAsync();
            await service.Files.Delete(id).ExecuteAsync();
        }

        public async Task<File> RenameFileAsync(string id, string newName)
        {
            var service = await GetServiceAsync();

            var meta = new File { Name = newName };
            var request = service.Files.Update(meta, id);
            request.Fields = "id, name";

            return await request.ExecuteAsync();
        }

        public async Task DownloadFileAsync(string localTarget, string id)
        {
            var service = await GetServiceAsync();

            using var stream = new FileStream(localTarget, FileMode.Create, FileAccess.Write);

            var request = service.Files.Get(id);
            await request.DownloadAsync(stream);
        }
    }
}