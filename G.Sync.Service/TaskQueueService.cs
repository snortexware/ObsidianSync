using G.Sync.Common;
using G.Sync.Repository;

namespace G.Sync.Service
{
    public class TaskQueueService
    {
        private readonly TaskQueueRepository _repository = new();
        private readonly FolderFileService _folderService = new();

        private const int FileLockMaxRetries = 10;
        private const int FileLockDelayMs = 500;

        public async void ProcessTaskQueues()
        {
            try
            {
                ProcessTaskQueuesInternal();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing task queues: {ex.Message}");
            }
        }

        private async void ProcessTaskQueuesInternal()
        {
            var taskQueues = _repository.GetTaskQueues()?.ToList();

            if (taskQueues == null || taskQueues.Count == 0) return;

            foreach (var taskQueue in taskQueues)
            {
                var realPath = taskQueue.FilePath;

                if (!WaitForFileReady(realPath) || !(await Helpers.HasFileSettledAsync(realPath)))
                {
                    Console.WriteLine($"Skipping task {taskQueue.Id} ({taskQueue.Type}) - file locked or tmp.");
                    continue;
                }

                bool success = false;
                try
                {
                    switch (taskQueue.Type)
                    {
                        case Entities.TaskEntity.TaskTypes.ChangeFile:
                            _folderService.UpdateFile(taskQueue.LocalRoot, realPath, taskQueue.DriveRoot);
                            success = true;
                            break;

                        case Entities.TaskEntity.TaskTypes.UploadFile:
                            _folderService.UploadFile(taskQueue.LocalRoot, realPath, taskQueue.DriveRoot);
                            success = true;
                            break;

                        case Entities.TaskEntity.TaskTypes.CreateFile:
                            _folderService.UploadFile(taskQueue.LocalRoot, realPath, taskQueue.DriveRoot);
                            success = true;
                            break;
                        case Entities.TaskEntity.TaskTypes.DeleteFile:
                            _folderService.DeleteFile(taskQueue.LocalRoot, realPath, taskQueue.DriveRoot);
                            success = true;
                            break;
                        case Entities.TaskEntity.TaskTypes.RenameFile:
                            _folderService.RenameFile(taskQueue.LocalRoot, taskQueue.OldPath, taskQueue.NewPath, taskQueue.DriveRoot);
                            success = true;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing task {taskQueue.Id} ({taskQueue.Type}): {ex.Message}");
                    success = false;
                }

                if (success)
                {
                    _repository.RemoveTaskQueue(taskQueue.Id);
                    Console.WriteLine($"Processed queued task {taskQueue.Id} ({taskQueue.Type})");
                }
                else
                {
                    Console.WriteLine($"Task {taskQueue.Id} ({taskQueue.Type}) remains in queue for retry.");
                }
            }
        }

        private static bool WaitForFileReady(string path, int maxRetries = FileLockMaxRetries, int delayMs = FileLockDelayMs)
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                if (!IsFileLocked(path))
                    return true;

                Thread.Sleep(delayMs);
            }
            return false;
        }

        private static bool IsFileLocked(string path)
        {
            if (Helpers.IsFileInUse(path))
                return true;

            if (!File.Exists(path))
                return false;

            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                return false;
            }
            catch
            {
                return true;
            }
        }
    }
}
