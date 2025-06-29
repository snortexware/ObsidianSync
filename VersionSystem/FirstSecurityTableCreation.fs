namespace VersionCore.FirstSystemTablesCreation
open VersionSystem
open VersionCore.CoreEngine.Registry
open Database.Connection
open Dapper


type FirstSecurityTableCreation() as this = 
    do register (this :> IVersionCore)
    interface IVersionCore with
        member _.Run() = 
            let connection = GlobalDbConnection.Instance.Connection
            connection.Execute("""
            CREATE TABLE IF NOT EXISTS SECURITY (
            KEY TEXT PRIMARY KEY, TOKEN TEXT NOT NULL, CREATEDAT TEXT NOT NULL
                )
            """) |> ignore


