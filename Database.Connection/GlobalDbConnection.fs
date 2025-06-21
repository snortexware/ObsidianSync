namespace Database.Connection
open System.Data.SqlTypes
open Dapper
open System.IO
open System
open Microsoft.Data.Sqlite

type GlobalDbConnection() =
    let dbPath = @"C:\obsidian-sync\sync.db"
    let dbFolder = Path.GetDirectoryName(dbPath)
    do
        if not (Directory.Exists(dbFolder)) then
            Directory.CreateDirectory(dbFolder) |> ignore

    let cnnString = $"Data Source={dbPath}"
    let connection = new SqliteConnection(cnnString)

    static let mutable instance: Lazy<GlobalDbConnection> option = None

    static member Start() = 
           if instance.IsNone then
            instance <- Some (lazy (GlobalDbConnection()))

    static member Instance =
        match instance with
        | Some inst -> inst.Value
        | None -> failwith "A conexão com o banco falhou."

    member _.Connection =
        if connection.State <> Data.ConnectionState.Open then
            connection.Open()
        connection

    member _.Close() =
        if connection.State <> Data.ConnectionState.Closed then
            connection.Close()



