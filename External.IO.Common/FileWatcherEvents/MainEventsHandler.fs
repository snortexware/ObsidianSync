module MainEvents

open System.Collections.Concurrent
open System.Security.Cryptography
open External.IO.Common.FileWatcherEvents
open System.IO
open External.IO.Common.FileWatcherEvents.MainEventsHandler

open Helper
open TaskCreation
open TaskProcess
open FileProcess
    
let private recentHashes = ConcurrentDictionary<string, byte[]>()
 
[<Literal>]
let private  _localRoot = @"C:\obsidian-sync"
let private _drive = UserConnection.ApiConnection.Instance.Service
let private _driveRoot = GetOrCreateRootFolder(_drive)
let private timestamp = System.DateTime.UtcNow

type MainEventsHandler() =
     do
        DownloadAllFiles(_drive, _driveRoot, _localRoot)  |> Async.RunSynchronously
        
     interface IMainEventsHandler with
         member this.ChangedEventHandler(e: FileSystemEventArgs) =
                let task = PrepareTask(e.Name, e.FullPath)
                
                if(task) then
                    let newHash = GetFileHash(e.FullPath)
                    match recentHashes.TryGetValue(e.FullPath) with
                    | true, oldHash when oldHash = newHash -> ()
                    | _ ->
                    recentHashes.[e.FullPath] <- newHash
                    use tc = new TaskWrapper(e.Name)
                    UpdateFile(_drive, e.FullPath, _localRoot, _driveRoot) |> ignore
                    tc.Complete()
                    printfn $"[{timestamp}] MODIFICADO {e.Name}"
                
         member this.CreatedEventHandler(e: FileSystemEventArgs) =
                let task = PrepareTask(e.Name, e.FullPath)
                
                if(task) then
                    let hash = GetFileHash e.FullPath
                    recentHashes.[e.FullPath] <- hash
                    use tc = new TaskWrapper(e.Name)
                    UploadFile(_drive, e.FullPath, _localRoot, _driveRoot) |> ignore
                    tc.Complete()
                    printfn $"[{timestamp}] CRIADO {e.Name}"
                    
         member this.DeletedEventHandler(e: FileSystemEventArgs) =
                let task = PrepareTask(e.Name, e.FullPath)
                
                if not (isInternalFile e.Name) && task then
                    use tc = new TaskWrapper(e.Name)
                    DeleteFile(_drive, e.FullPath, _localRoot, _driveRoot) |> ignore
                    tc.Complete()
                    printfn $"[{timestamp}] REMOVIDO {e.Name}"
               
                    
         member this.RenamedEventHandler(e: RenamedEventArgs) =
                let task = PrepareTask(e.Name, e.FullPath)
                if not (isInternalFile e.Name) && task then
                     let dTo = InitialTaskProcess(e.Name)
                     use tc = new TaskWrapper(e.Name)
                     RenameFile(_drive, e.OldFullPath, e.FullPath, _localRoot, _driveRoot) |> ignore
                     tc.Complete()
                     printfn $"[{timestamp}] RENOMEADO {e.OldName} → {e.Name}"
       
                

