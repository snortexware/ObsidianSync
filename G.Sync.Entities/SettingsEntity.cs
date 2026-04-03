using G.Sync.Entities.Interfaces;

namespace G.Sync.Entities
{
    public class SettingsEntity : ISettingsEntity
    {
        public long Id { get; private set; }
        public string? Folder { get; private set; }
        public string? DriveProjectName { get; private set; }
        public string? GoogleDriveFolderName { get; private set; }
        public string IpAdress { get; private set; }
        public int Port { get; private set; }

        public static SettingsEntity CreateSettings(string driveProjetoName, string googleDriveFolderName, string folder, string ipAdress, int port)
        {
            return new SettingsEntity
            {
                Folder = folder, 
                DriveProjectName = driveProjetoName,
                GoogleDriveFolderName = googleDriveFolderName,
                IpAdress = ipAdress,
                Port = port
            };
        }

        public void UpdateFolder(string folder)
        {
            Folder = folder;
        }

    }
}
