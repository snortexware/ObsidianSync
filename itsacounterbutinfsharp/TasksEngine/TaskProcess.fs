module TaskProcess
open Database.Connection
open System
open Dapper

type TaskWrapper(fileId: string) =
    let mutable completed = false
    let connection = GlobalDbConnection.Instance.Connection
    let sql = "UPDATE TASKS SET COMPLETED = 1 WHERE FILEID = @ID AND (COMPLETED <> 1 OR COMPLETED IS NULL)"

    member this.Complete() =
        completed <- true

    interface IDisposable with
        member _.Dispose() =

            if completed then
                let parameters = dict [ "ID", box fileId ]
                connection.Execute(sql, parameters) |> ignore

