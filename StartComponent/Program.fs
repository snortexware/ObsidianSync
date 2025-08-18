namespace MyFSharpLib
open System.Security.Cryptography
open System.Collections.Concurrent
open SystemDataContracts
open UserInput
open UserConnection
open VersionCore.CoreEngine
open Database.Connection
open TaskCreation
open TaskProcess
open External.IO.Common.FileWatcherEvents.HotFileWatcher

type MainStartProcess() =
    [<Literal>]
    let localRoot = @"C:\obsidian-sync"

    member this.Start() =
        GlobalDbConnection.Start() |> ignore

        AutoRegister.autoRegisterAll()
        RegistryCore.runAll()

        let appC = UserInput()
        let userApp = appC.UserProjetoName

        UserConnection.ApiConnection.Init(userApp)
        HotWatcher.Instance.Value.Watch()
    
        System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite)

