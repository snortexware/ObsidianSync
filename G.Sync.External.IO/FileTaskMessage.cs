using static G.Sync.Entities.TaskEntity;

public class FileTaskMessage
{
    public string Path { get; set; } = string.Empty;
    public TaskTypes TaskType { get; set; }
    public DateTime Timestamp { get; set; }
}
