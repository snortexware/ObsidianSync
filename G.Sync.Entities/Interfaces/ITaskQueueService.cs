namespace G.Sync.Entities.Interfaces
{
    public interface ITaskQueueService
    {
        Task ProcessTaskQueues();
    }
}