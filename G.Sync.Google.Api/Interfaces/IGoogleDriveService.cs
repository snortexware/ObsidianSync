using Google.Apis.Drive.v3.Data;
using Google.Apis.Upload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using File = Google.Apis.Drive.v3.Data.File;
namespace G.Sync.Google.Interfaces
{
    public interface IGoogleDriveService
    {
        FileList ListFiles(string query, string fields, string spaces = "drive");
        FileList ListFilesAsync(string query, string fields, string spaces = "drive");
        File CreateFolder(File folderMeta, string fields);
        (File responseBody, IUploadProgress  progress) UploadFile(string parentId, string localPath);
        (File responseBody, IUploadProgress  progress) UpdateFile(string id, string localPath);
        void DeleteFile(string id);
        File RenameFile(string id, string newName);
        void DownloadFile(string localTarget, string id);
    }
}
