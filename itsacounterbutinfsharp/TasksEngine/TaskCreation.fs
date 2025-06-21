module TaskCreation

open Database.Connection
open Dapper
open SystemDataContracts

type RegisterTask() =
    let mutable taskDataOpt: TaskDataObject option = None

    member _.Data(data: TaskDataObject) =
        taskDataOpt <- Some data

    member _.Run() =
        match taskDataOpt with
        | Some taskData ->
            let connection = GlobalDbConnection.Instance.Connection
            let parameters = dict [ "ID", box taskData.FileId ]
            let sqlTaskExist = "SELECT ID FROM TASKS WHERE FILEID = @ID AND COMPLETED <> 1"
            let tasksExist = connection.QueryFirstOrDefault(sqlTaskExist, parameters)

            if isNull tasksExist then
                let sql = """
                    INSERT INTO TASKS (NAME, EXECUTETS, FILEID)
                    VALUES (@Name, @ExecuteTs, @FileId)
                """
                connection.Execute(sql, taskData) |> ignore
            else
                printfn "Tarefa já existe."
        | None ->
            printfn "Nenhum dado fornecido. Use .Data(taskData) antes de chamar .Run()."