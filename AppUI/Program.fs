module MainStartProcess
open SystemDataContracts
open System
open System.IO
open FileProcess
open UserInput
open UserConnection
open VersionSystem
open VersionCore.CoreEngine
open Database.Connection
open Dapper
open TaskCreation
open TaskProcess

[<Literal>]
let defaultPath = @"C:\obsidian-sync"

let InitialTaskProcess (fileId: string) =
    let timeExecute = DateTime.UtcNow
    let dataContract = TaskDataObject()
    dataContract.Name <- "FileProcess"
    dataContract.FileId <- fileId
    dataContract.ExecuteTs <- timeExecute.ToString()
    dataContract

let main () =

    GlobalDbConnection.Start() |> ignore

    AutoRegister.autoRegisterAll()
    Registry.runAll()


    let appC = UserInput()
    let userApp = appC.UserProjetoName

    UserConnection.ApiConnection.Init(userApp)
    let cc = UserConnection.ApiConnection.Instance
    let drive = cc.Service

    let defaultFolder = GetOrCreateDefaultFolder(drive)

    let fileWatcher = new FileSystemWatcher(defaultPath)
    fileWatcher.NotifyFilter <- NotifyFilters.FileName ||| NotifyFilters.LastWrite
    fileWatcher.IncludeSubdirectories <- false
    fileWatcher.EnableRaisingEvents <- true
    fileWatcher.Filter <- "*.*"

    fileWatcher.Changed.Add(fun e ->
        try
         if e.Name.EndsWith(".db-journal") || e.Name.EndsWith("~") then
            () 
         else
            let dataContract = InitialTaskProcess(e.Name)
            let task = RegisterTask()
            task.Data(dataContract)
            task.Run()

            use tc = new TaskWrapper(e.Name)
            let wasUpdated = UploadChangedFile(drive, e.FullPath, e.Name)
            tc.Complete()

            if wasUpdated.IsSome then
                printfn "Arquivo atualizado: %s" e.FullPath
        with ex ->
            printfn "Erro ao processar mudança no arquivo: %s\n%s" e.FullPath ex.Message
    )

    fileWatcher.Created.Add(fun e ->
    if e.Name.EndsWith(".db-journal") || e.Name.EndsWith("~") then
        () 
    else    
        try
            let dataContract = InitialTaskProcess(e.Name)
            let task = RegisterTask()
            task.Data(dataContract)
            task.Run()

            use tc = new TaskWrapper(e.Name)
            let wasUpdated = UploadFile(drive, e.FullPath, defaultFolder)
            tc.Complete()
            printfn "📝 Arquivo atualizado: %s" e.FullPath

        with ex ->
            printfn "⚠️ Erro ao processar mudança no arquivo: %s\n%s" ex.Message ex.StackTrace )

    System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite)





