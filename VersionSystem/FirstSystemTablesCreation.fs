namespace VersionCore.FirstSystemTablesCreation
open VersionSystem
open VersionCore.CoreEngine.Registry
open Database.Connection
open Dapper


type FirstSystemTablesCreation() as this = 
    do register (this :> IVersionCore)
    interface IVersionCore with
        member _.Run() = 
            let connection = GlobalDbConnection.Instance.Connection
            connection.Execute("""
            CREATE TABLE IF NOT EXISTS TASKS (
            Id INTEGER PRIMARY KEY AUTOINCREMENT, NAME TEXT NOT NULL, EXECUTETS TEXT NOT NULL, COMPLETED INTEGER DEFAULT 0, FILEID TEXTO NOT NULL, TASKTYPE INTEGER
                )
            """) |> ignore


