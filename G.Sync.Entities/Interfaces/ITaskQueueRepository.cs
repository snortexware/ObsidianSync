using G.Sync.Entities;

public interface ITaskQueueRepository
{
    IEnumerable<TaskQueue> GetTaskQueues();
    void AddTaskQueue(TaskQueue taskQueue);
    void RemoveTaskQueue(long id);
    bool IsFileInQueue(string path);
}