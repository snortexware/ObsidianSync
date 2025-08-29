using Dapper;
using G.Sync.DataContracts;
using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using G.Sync.Repository;
using G.Sync.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace G.Sync.TasksManagment
{
    public class TaskCreation(ITaskRepository taskRepo)
    {
        private ITaskRepository TaskRepo { get; set; } = taskRepo;
        private TaskEntity? _task;

        public void Data(TaskEntity entity)
        {
            _task = entity;
        }

        public void CreateTask()
        {
            if (_task is null)
                throw new Exception("The data of the task was not find, call Data() before calling .Run().");

            var parameters = new DynamicParameters();
            parameters.Add("ID", _task.FileId);
            parameters.Add("completed", StatusType.completed);

            const string sqlTaskExist = "SELECT ID FROM TASKS WHERE FILEID = @ID AND STATUS <> @completed";

            var taskExist = TaskRepo.GetFirstOrDefault(sqlTaskExist, parameters);

            if (taskExist is null)
                taskRepo.Save(_task);
        }

    }
}

