namespace G.Sync.Entities.Interfaces
{
    public interface IFolderFileProcess
    {
        ISettingsEntity Settings { get; set; }
        Task<string> DeleteFileAsync(string localRoot, string filePath, string driveRoot);
        Task DownloadAllFilesAsync(string driveRoot, string vaultPath);
        Task<string> EnsureDriveFolderAsync(string relPath, string rootId);
        Task<string> FileExistsAsync(string fileName, string parentId);
        Task<string> GetOrCreateRootFolderAsync();
        Task<string> RenameFileAsync(string localRoot, string oldPath, string newPath, string driveRoot);
        Task<string> UpdateFileAsync(string localRoot, string filePath, string driveRoot);
        Task<string> UploadFileAsync(string localRoot, string filePath, string driveRoot);
    }
}