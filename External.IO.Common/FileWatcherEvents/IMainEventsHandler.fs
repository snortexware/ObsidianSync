namespace External.IO.Common.FileWatcherEvents.MainEventsHandler

open System.IO

type IMainEventsHandler =
     abstract member ChangedEventHandler : FileSystemEventArgs  -> unit
     abstract member CreatedEventHandler : FileSystemEventArgs  -> unit
     abstract member RenamedEventHandler : RenamedEventArgs  -> unit
     abstract member DeletedEventHandler : FileSystemEventArgs  -> unit