namespace External.IO.Common.FileWatcherEvents.HotFileWatcher
open System.IO
open External.IO.Common.FileWatcherEvents.MainEventsHandler
open MainEvents

type public HotWatcher() =
    let _localRoot = @"C:\obsidian-sync"

    member _.Watch() =
        let events = MainEventsHandler() :> IMainEventsHandler
        let watcher = new FileSystemWatcher(_localRoot)
        watcher.IncludeSubdirectories <- true
        watcher.NotifyFilter <- NotifyFilters.FileName ||| NotifyFilters.LastWrite
        watcher.Filter <- "*.*"
        
        try
            watcher.Created.Add(events.CreatedEventHandler)
            watcher.Deleted.Add(events.DeletedEventHandler)
            watcher.Renamed.Add(events.RenamedEventHandler)
            watcher.Changed.Add(events.ChangedEventHandler)
        with ex -> 
            printf $"ERRO NOS EVENTOS DO WATCHER: {ex.Message} {ex.StackTrace} "
            
        watcher.EnableRaisingEvents <- true
        
        printfn $"Monitorando alterações em: {_localRoot}"

        System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite)


    static member val Instance = lazy (HotWatcher()) with get