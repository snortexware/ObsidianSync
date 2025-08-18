module Helper

open System
open System.IO
open System.Security.Cryptography
open SystemDataContracts
open TaskCreation
open Utils.NotifyHandler

let isInternalFile (name: string) =
        name.Contains("sync.db") || name.Contains("sync.db-journal")

let GetFileHash (path: string) =
    try
        use fs = File.OpenRead(path)
        use sha = SHA256.Create()
        sha.ComputeHash(fs)
    with _ -> [||]
    
let InitialTaskProcess (fileId: string) =
    let timeExecute = DateTime.UtcNow
    let dataContract = TaskDataObject()
    dataContract.Name <- "FileProcess"
    dataContract.FileId <- fileId
    dataContract.ExecuteTs <- timeExecute.ToString()
    dataContract
    
let PrepareTask(name: string, path: string) =
            if File.Exists(path) && not (isInternalFile name) then
                let dTo = InitialTaskProcess(name);

                async {
                        try
                        printfn "Preparando tarefa"
                        do! NotifyHandler.notifyHandler.NotifyClientsAsync(dTo) |> Async.AwaitTask
                        with ex ->
                        printfn "Error notifying clients: %A" ex
                        } |> Async.StartImmediate

                let task = RegisterTask()
                task.Data(dTo)
                task.Run()
                true
            else
                false    
