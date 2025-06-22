module MainStartProcess

open System
open System.IO
open System.Security.Cryptography
open System.Collections.Concurrent
open FileProcess
open SystemDataContracts
open UserInput
open UserConnection
open VersionSystem
open VersionCore.CoreEngine
open Database.Connection
open Dapper
open TaskCreation
open TaskProcess

[<Literal>]
let localRoot = @"C:\obsidian-sync"

let InitialTaskProcess (fileId: string) =
    let timeExecute = DateTime.UtcNow
    let dataContract = TaskDataObject()
    dataContract.Name <- "FileProcess"
    dataContract.FileId <- fileId
    dataContract.ExecuteTs <- timeExecute.ToString()
    dataContract

let isInternalFile (name: string) =
    name.Contains("sync.db") || name.Contains("sync.db-journal")

let getFileHash (path: string) =
    try
        use fs = File.OpenRead(path)
        use sha = SHA256.Create()
        sha.ComputeHash(fs)
    with _ -> [||]

let main () =
    GlobalDbConnection.Start() |> ignore

    AutoRegister.autoRegisterAll()
    Registry.runAll()

    let appC = UserInput()
    let userApp = appC.UserProjetoName

    UserConnection.ApiConnection.Init(userApp)
    let drive = UserConnection.ApiConnection.Instance.Service

    let driveRoot = GetOrCreateRootFolder(drive)

    DownloadAllFiles(drive, driveRoot, localRoot) |> Async.RunSynchronously

    let recentHashes = ConcurrentDictionary<string, byte[]>()

    let watcher = new FileSystemWatcher(localRoot)
    watcher.IncludeSubdirectories <- true
    watcher.NotifyFilter <- NotifyFilters.FileName ||| NotifyFilters.LastWrite
    watcher.Filter <- "*.*"
    watcher.EnableRaisingEvents <- true

    watcher.Changed.Add(fun e ->
        if File.Exists(e.FullPath) && not (isInternalFile e.Name) then
            let newHash = getFileHash e.FullPath
            match recentHashes.TryGetValue(e.FullPath) with
            | true, oldHash when oldHash = newHash -> ()
            | _ ->
                recentHashes.[e.FullPath] <- newHash
                let dTo = InitialTaskProcess(e.Name)
                let task = RegisterTask()
                task.Data(dTo)
                task.Run()
                use tc = new TaskWrapper(e.Name)
                UpdateFile(drive, e.FullPath, localRoot, driveRoot) |> ignore
                tc.Complete()
                printfn $"[MODIFICADO] {e.Name}"
    )

    watcher.Created.Add(fun e ->
        if File.Exists(e.FullPath) && not (isInternalFile e.Name) then
            let hash = getFileHash e.FullPath
            recentHashes.[e.FullPath] <- hash
            let dTo = InitialTaskProcess(e.Name)
            let task = RegisterTask()
            task.Data(dTo)
            task.Run()
            use tc = new TaskWrapper(e.Name)
            UploadFile(drive, e.FullPath, localRoot, driveRoot) |> ignore
            tc.Complete()
            printfn $"[CRIADO] {e.Name}"
    )

    watcher.Deleted.Add(fun e ->
        if not (isInternalFile e.Name) then
            let dTo = InitialTaskProcess(e.Name)
            let task = RegisterTask()
            task.Data(dTo)
            task.Run()
            use tc = new TaskWrapper(e.Name)
            DeleteFile(drive, e.FullPath, localRoot, driveRoot) |> ignore
            tc.Complete()
            printfn $"[REMOVIDO] {e.Name}"
    )

    watcher.Renamed.Add(fun e ->
        if not (isInternalFile e.Name) then
            let dTo = InitialTaskProcess(e.Name)
            let task = RegisterTask()
            task.Data(dTo)
            task.Run()
            use tc = new TaskWrapper(e.Name)
            RenameFile(drive, e.OldFullPath, e.FullPath, localRoot, driveRoot) |> ignore
            tc.Complete()
            printfn $"[RENOMEADO] {e.OldName} → {e.Name}"
    )

    printfn $"Monitorando alterações em: {localRoot}"
    System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite)
