namespace G.Sync.Entities
{
    public partial class TaskEntity
    {
        public string? FileId { get; private set; }
        public string? Id { get; }
        public string Name { get; private set; }
        public string CreatedAt { get; private set; }
        public TasksStatus Status { get; private set; }
        public TaskTypes TaskType { get; private set; }

        public TaskEntity CreateTask(string fileId, TaskTypes taskType)
        {
            switch (taskType)
            {
                case TaskTypes.UploadFile:
                    this.Name = "Upload File";
                    break;
                case TaskTypes.DownloadFile:
                    this.Name = "Download File";
                    break;
                case TaskTypes.DeleteFile:
                    this.Name = "Delete File";
                    break;
                case TaskTypes.RenameFile:
                    this.Name = "Rename File";
                    break;
            }

            this.TaskType = taskType;
            this.CreatedAt = DateTime.UtcNow.ToString();
            this.Status = TasksStatus.Pending;
            this.FileId = fileId;

            return this;
        }

        public void MarkAsCompleted() => this.Status = TasksStatus.Completed;
        public void MarkAsFailed() => this.Status = TasksStatus.Failed;
        public enum TasksStatus
        {
            Pending = 1,
            Completed = 2,
            Failed = 3,
        }

        public enum TaskTypes
        {
            UploadFile = 0,
            DownloadFile = 1,
            DeleteFile = 2,
            RenameFile = 3
        }
    }
}
