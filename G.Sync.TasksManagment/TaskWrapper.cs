using G.Sync.DataContracts;
using G.Sync.Repository;
using G.Sync.Service;
using static G.Sync.Entities.TaskEntity;

namespace G.Sync.TasksManagment
{
    public class TaskWrapper(string fileId) : IDisposable
    {
        private readonly string _fileId = fileId;

        private bool _completed = false;

        public void Complete()
        {
            _completed = true;
        }

        public void Dispose()
        {
            var taskRepo = new TaskRepository();

            var taskService = new TaskService(taskRepo);

            try
            {
                if (_completed)
                {
                    taskService.UpdateTaskStatus(_fileId, TasksStatus.Completed);
                }

            }
            catch (Exception ex)
            {
                taskService.UpdateTaskStatus(_fileId, TasksStatus.Failed);

                throw new Exception("Failed to update task status to completed.", ex);
            }
        }
    }
}
