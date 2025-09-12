using System.IO;

namespace G.Sync.Entities.Interfaces
{
    public interface IEventsHandler
    {
        public void ChangedEventHandler(object sender, FileSystemEventArgs e);
        public void CreatedEventHandler(object sender, FileSystemEventArgs e);
        public void RenamedEventHandler(object sender, RenamedEventArgs e);
        public void DeletedEventHandler(object sender, FileSystemEventArgs e);
    }
}
