using G.Sync.Entities;

public interface ITaskQueueRepository
{
    IEnumerable<TaskQueue> GetTaskQueues();
}