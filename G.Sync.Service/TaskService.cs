
using G.Sync.Entities.Interfaces;

public class TaskService(ITaskRepository taskRepo)
{
    private readonly ITaskRepository _taskRepo = taskRepo;

    public void UpdateTaskStatus(int id, TaskStatus newStatus)
    {
        var task = _taskRepo.GetById(id);
        if (task == null) throw new Exception("Task not found");

        task.Status = (int)newStatus;

        _taskRepo.Save(task);
    }
}
