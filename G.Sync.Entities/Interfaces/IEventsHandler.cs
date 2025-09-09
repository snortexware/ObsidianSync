using System.IO;

namespace G.Sync.Entities.Interfaces
{
    public interface IEventsHandler
    {
        public void ChangedEventHandler(FileSystemEventArgs e);
        public void CreatedEventHandler(FileSystemEventArgs e);
        public void RenamedEventHandler(RenamedEventArgs e);
        public void DeletedEventHandler(FileSystemEventArgs e);
    }
}
