using Google.Apis.Drive.v3.Data;
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
        File CreateFolder(string name, string parentId);
        File UploadFile(string parentId, string localPath);
        File UpdateFile(string id, string localPath);
        void DeleteFile(string id);
        File RenameFile(string id, string newName);
    }
}
