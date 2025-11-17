using Dapper;
using G.Sync.DataContracts;
using G.Sync.Entities;
using G.Sync.Entities.Interfaces;

public class TaskCreation
{
    private readonly ITaskRepository _taskRepo;
    private TaskEntity? _task;
    public TaskCreation(ITaskRepository taskRepo)
    {
        _taskRepo = taskRepo;
    }

    public void Data(TaskEntity entity)
    {
        _task = entity;
    }

    public (bool isPrepared, long taskId) SaveTask()
    {
        if (_task is null)
            throw new Exception("The data of the task was not found. Call Data() before CreateTask().");

        var taskId = _taskRepo.Save(_task);

        return (true, taskId);
    }
}
