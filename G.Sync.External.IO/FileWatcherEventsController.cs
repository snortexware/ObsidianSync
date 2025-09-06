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

        private FileWatcherEventsController() { }

        public static FileWatcherEventsController Instance => _lazyInstance.Value;

        public void StartWatching(SettingsEntity settings)
        {  
            _events = new EventsHandler(settings);
            _fileWatcher = new FileSystemWatcher(settings.GoogleDriveFolderName)
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                Filter = "*.*",
                EnableRaisingEvents = true
            };

            _fileWatcher.Created += _events.CreatedEventHandler;
            _fileWatcher.Deleted += _events.DeletedEventHandler;
            _fileWatcher.Renamed += _events.RenamedEventHandler;
            _fileWatcher.Changed += _events.ChangedEventHandler;
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
