using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using G.Sync.Repository;
using System;
using System.IO;
using System.Collections.Generic;
using G.Sync.IoC;

namespace G.Sync.External.IO
{
    public class FileWatcherEventsController : IFileWatcher
    {
        private readonly Dictionary<string, FileSystemWatcher> _watchers = [];
        private readonly Dictionary<string, IEventsHandler> _events = [];

        public FileWatcherEventsController() { }

        public async void StartWatching(IEnumerable<VaultsEntity> vaults, ISettingsEntity settings)
        {
            StopWatching();

            foreach (var vault in vaults)
            {
                Console.WriteLine($"Watching vault: {vault.Path}");

                var events = BusinessComponent.CreateInstance<IEventsHandler>();

                await events.InitializeAsync(vault.Id, settings);

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
                _events[vault.Path] = events;
            }
        }

        public void StopWatching()
        {
            foreach (var path in _watchers.Keys)
            {
                var watcher = _watchers[path];
                var events = _events[path];

                watcher.EnableRaisingEvents = false;
                watcher.Created -= events.CreatedEventHandler;
                watcher.Deleted -= events.DeletedEventHandler;
                watcher.Renamed -= events.RenamedEventHandler;
                watcher.Changed -= events.ChangedEventHandler;

                watcher.Dispose();
            }

            _watchers.Clear();
            _events.Clear();
        }
    }
}