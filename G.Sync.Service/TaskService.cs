
using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using static G.Sync.Entities.TaskEntity;

namespace G.Sync.Service
{
    public class TaskService(ITaskRepository taskRepo)
    {
        private readonly ITaskRepository _taskRepo = taskRepo;

        public void UpdateTaskStatus(string id, TasksStatus newStatus)
        {
            var task = _taskRepo.GetById(id);

            if (task == null) throw new Exception("Task not found");

            task.Status = (int)newStatus;

            _taskRepo.Save(task);
        }
    }

}