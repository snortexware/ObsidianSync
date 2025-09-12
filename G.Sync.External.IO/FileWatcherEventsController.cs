using G.Sync.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.External.IO
{
    public class FileWatcherEventsController
    {
        private readonly Lazy<FileWatcherEventsController> _lazyInstance = new Lazy<FileWatcherEventsController>(() => new FileWatcherEventsController());
        private FileWatcherEventsController() { }

        public FileWatcherEventsController GetInstance
        {
            get
            {
                return _lazyInstance.Value;
            }
        }

        public void StartWatching()
        {
            var settingsRepo = new SettingsRepository();
            var settings = settingsRepo.GetSettings();

            var events = new EventsHandler(settings);
            var fileWatcher = new FileSystemWatcher(settings.GoogleDriveFolderName);
            fileWatcher.IncludeSubdirectories = true;
            fileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            fileWatcher.Filter = "*.*";

            try
            {
                fileWatcher.Created += events.CreatedEventHandler;
                fileWatcher.Deleted += events.DeletedEventHandler;
                fileWatcher.Renamed += (events.RenamedEventHandler);
                fileWatcher.Changed += (events.ChangedEventHandler);
            }
            catch (Exception ex)
            {
                throw new Exception("ERRO NOS EVENTOS DO WATCHER", ex);
            }

            fileWatcher.EnableRaisingEvents = true;
        }
    }

        //        namespace External.IO.Common.FileWatcherEvents.HotFileWatcher
        //open System.IO
        //open External.IO.Common.FileWatcherEvents.MainEventsHandler
        //open MainEvents

        //type public HotWatcher() =
        //    let _localRoot = @"C:\obsidian-sync"

        //    member _.Watch() =
        //        let events = MainEventsHandler() :> IMainEventsHandler
        //        let watcher = new FileSystemWatcher(_localRoot)
        //        watcher.IncludeSubdirectories<- true
        //        watcher.NotifyFilter<- NotifyFilters.FileName ||| NotifyFilters.LastWrite
        //        watcher.Filter<- "*.*"

        //        try
        //            watcher.Created.Add(events.CreatedEventHandler)
        //            watcher.Deleted.Add(events.DeletedEventHandler)
        //            watcher.Renamed.Add(events.RenamedEventHandler)
        //            watcher.Changed.Add(events.ChangedEventHandler)
        //        with ex -> 
        //            printf $"ERRO NOS EVENTOS DO WATCHER: {ex.Message} {ex.StackTrace} "



        //        printfn $"Monitorando alterações em: {_localRoot}"

        //        System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite)


        //    static member val Instance = lazy(HotWatcher()) with get
    }
