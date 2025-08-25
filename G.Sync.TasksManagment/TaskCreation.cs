using Dapper;
using G.Sync.DataContracts;
using G.Sync.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace G.Sync.TasksManagment
{
    public static class TaskCreation
    {
        public static void Run(TaskDto taskData)
        {
            if (taskData is null)
                throw new Exception("The data of the task was not find, call Data() before calling .Run().");

            using (var tc = new TransactionContext())
            {
                var cnn = tc.Connection;

                var parameters = new DynamicParameters();
                parameters.Add("ID", taskData.FileId);
                parameters.Add("completed", StatusType.completed);

                const string sqlTaskExist = "SELECT ID FROM TASKS WHERE FILEID = @ID AND STATUS <> @completed";

                var tasksExist = cnn.Query<TaskDto>(sqlTaskExist, parameters).FirstOrDefault();

                if (tasksExist is null)
                {
                    var sql = @"
                    INSERT INTO TASKS (NAME, EXECUTETS, FILEID, TASKTYPE)
                    VALUES (@Name, @ExecuteTs, @FileId, @TaskType)
                ";

                    cnn.Execute(sql, taskData);
                }

                tc.Complete();
            }


        }

    }
}

