using System.IO;

namespace G.Sync.Entities.Interfaces
{
    public interface IEventsHandler
    {
        Task InitializeAsync(long vaultId, ISettingsEntity settings);
        public void ChangedEventHandler(object sender, FileSystemEventArgs e);
        public void CreatedEventHandler(object sender, FileSystemEventArgs e);
        public void RenamedEventHandler(object sender, RenamedEventArgs e);
        public void DeletedEventHandler(object sender, FileSystemEventArgs e);
    }
}
