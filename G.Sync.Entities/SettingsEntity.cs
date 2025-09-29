using G.Sync.Entities.Interfaces;

namespace G.Sync.Entities
{
    public class SettingsEntity : ISettingsEntity
    {
        public long Id { get; private set; }
        public string? Folder { get; private set; }
        public string? DriveProjectName { get; private set; }
        public string? GoogleDriveFolderName { get; private set; }

        public ISettingsEntity CreateSettings(string driveProjetoName, string googleDriveFolderName, string folder)
        {
            return new SettingsEntity
            {
                Folder = folder, 
                DriveProjectName = driveProjetoName,
                GoogleDriveFolderName = googleDriveFolderName
            };
        }

        public void UpdateFolder(string folder)
        {
            Folder = folder;
        }

    }
}
