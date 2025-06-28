module MainStartProcess
open External.IO.Common.FileWatcherEvents.HotFileWatcher
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

let main () =
    GlobalDbConnection.Start() |> ignore

    AutoRegister.autoRegisterAll()
    Registry.runAll()

    let appC = UserInput()
    let userApp = appC.UserProjetoName

    UserConnection.ApiConnection.Init(userApp)
    HotWatcher.Instance.Value.Watch()

    System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite)
