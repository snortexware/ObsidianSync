namespace VersionSystem.MainSyncFolder
open VersionSystem
open VersionCore.CoreEngine.RegistryCore
open System.IO

type MainSyncFolder() as this =
    do register(this :> IVersionCore)
    [<Literal>]
    let defaultPath = @"C:\obsidian-sync"
    interface IVersionCore with
        member _.Run() =
            let mutable pathExists = Directory.Exists(defaultPath)

            if not (pathExists) then
               Directory.CreateDirectory(defaultPath) |> ignore

