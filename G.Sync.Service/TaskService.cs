
using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using G.Sync.Google.Api;
using System.Data;
using static G.Sync.Entities.TaskEntity;

namespace G.Sync.Service
{
    public class TaskService(ITaskRepository taskRepo)
    {
        private readonly ITaskRepository _taskRepo = taskRepo;

        public void UpdateTaskStatus(string fileId, TasksStatus newStatus)
        {
            Console.WriteLine(fileId + newStatus.ToString());

            var task = _taskRepo.GetByFileId(fileId);

            if (task == null) throw new Exception("Task not found");

            task.UpdateStatusByFileId(fileId, newStatus);

            Console.WriteLine("salvou?" + task.Status);

            _taskRepo.Save(task);
        }
    }

}