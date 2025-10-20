using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static G.Sync.Entities.TaskEntity;

namespace G.Sync.Entities
{
    public class TaskQueue
    {
        public long Id { get; private set; }
        public string? LocalRoot { get; private set; }
        public string? OldPath { get; private set; }
        public string? NewPath { get; private set; }
        public string? DriveRoot { get; private set; }
        public TaskTypes Type { get; set; }
        public string? FilePath { get; set; }
        public void CreateTaskInQueue(string localRoot, string oldPath, string newPath, string driveRoot, string filePath, TaskTypes taskId)
        {
            LocalRoot = localRoot;
            OldPath = oldPath;
            NewPath = newPath;
            DriveRoot = driveRoot;
            FilePath = filePath;
            Type = taskId;
        }
    }
}
