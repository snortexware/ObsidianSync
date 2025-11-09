using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using G.Sync.Repository;
using System;
using System.IO;

namespace G.Sync.External.IO
{
    public class FileWatcherEventsController
    {
        private static readonly Lazy<FileWatcherEventsController> _lazyInstance = new Lazy<FileWatcherEventsController>(() => new FileWatcherEventsController());
        private FileSystemWatcher? _fileWatcher;
        private EventsHandler? _events;
        private readonly Dictionary<string, FileSystemWatcher> _watchers = [];

        private FileWatcherEventsController() { }

        public static FileWatcherEventsController Instance => _lazyInstance.Value;

        public void StartWatching(IEnumerable<VaultsEntity> vaults, SettingsEntity settings)
        {
            StopWatching();

            foreach (var vault in vaults)
            {
                Console.WriteLine($"Watching vault: {vault.Path}");

                var events = new EventsHandler(settings, vault.Path);

                var watcher = new FileSystemWatcher(vault.Path)
                {
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName,
                    Filter = "*.*",
                    EnableRaisingEvents = true
                };

                watcher.Created += events.CreatedEventHandler;
                watcher.Deleted += events.DeletedEventHandler;
                watcher.Renamed += events.RenamedEventHandler;
                watcher.Changed += events.ChangedEventHandler;

                _watchers[vault.Path] = watcher;
            }
        }


        public void StopWatching()
        {
            if (_fileWatcher != null && _events != null)
            {
                _fileWatcher.EnableRaisingEvents = false;

                _fileWatcher.Created -= _events.CreatedEventHandler;
                _fileWatcher.Deleted -= _events.DeletedEventHandler;
                _fileWatcher.Renamed -= _events.RenamedEventHandler;
                _fileWatcher.Changed -= _events.ChangedEventHandler;

                _fileWatcher.Dispose();
                _fileWatcher = null;
                _events = null;
            }
        }
    }
}
