namespace G.Sync.Entities
{
    public partial class TaskEntity
    {
        public string? FileId { get; private set; }
        public long Id { get; }
        public string Name { get; private set; }
        public string VaultName { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public TasksStatus Status { get; private set; }
        public TaskTypes TaskType { get; private set; }

        public TaskEntity CreateTask(string fileId, TaskTypes taskType, string vaultName)
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
                case TaskTypes.CreateFile:
                    this.Name = "Create File";
                    break;
            }
            if (string.IsNullOrEmpty(this.Name))
            {
                this.Name = "Unknown Task";
            }

            Console.WriteLine("passei aqui para alterar status");

            this.TaskType = taskType;
            this.CreatedAt = DateTime.UtcNow;
            this.Status = TasksStatus.Pending;
            this.FileId = fileId;
            this.VaultName = vaultName;

            return this;
        }

        public void CopyFrom(TaskEntity other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            this.FileId = other.FileId;
            this.Name = other.Name;
            this.CreatedAt = other.CreatedAt;
            this.Status = other.Status;
            this.TaskType = other.TaskType;
        }

        public void UpdateStatusByFileId(string fileId, TasksStatus newStatus)
        {
            if (this.FileId != fileId || string.IsNullOrEmpty(fileId)) throw new Exception("FileId mismatch or missing.");

            this.Status = newStatus;
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
            RenameFile = 3,
            CreateFile = 4,
            ChangeFile = 5  
        }
    }
}
