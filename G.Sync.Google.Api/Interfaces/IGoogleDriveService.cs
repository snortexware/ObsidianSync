using Google.Apis.Drive.v3.Data;
using Google.Apis.Upload;
using System.Threading.Tasks;
using File = Google.Apis.Drive.v3.Data.File;

namespace G.Sync.Google.Api.Interfaces
{
    public interface IGoogleDriveService
    {
        Task<FileList> ListFilesAsync(string query, string fields, string spaces = "drive");

        Task<File> CreateFolderAsync(File folderMeta, string fields);

        Task<(File responseBody, IUploadProgress progress)> UploadFileAsync(string parentId, string localPath);

        Task<(File responseBody, IUploadProgress progress)> UpdateFileAsync(string id, string localPath);

        Task DeleteFileAsync(string id);

        Task<File> RenameFileAsync(string id, string newName);

        Task DownloadFileAsync(string localTarget, string id);
    }
}