using G.Sync.Common;
using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using G.Sync.Google.Api;
using G.Sync.Repository;
using G.Sync.TasksManagment;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using static G.Sync.Entities.TaskEntity;

namespace G.Sync.External.IO
{
    public class EventsHandler : FolderFileProcess, IEventsHandler
    {
        private static readonly ConcurrentDictionary<string, byte[]> recentHashes = new();
        private readonly string _localRoot;
        private readonly string _driveRoot;
        private readonly DateTime timestamp = DateTime.UtcNow;
        private readonly long _vaultId;
        private readonly TaskQueueRepository _taskQueueRepository = new();
        private readonly ITaskNotifier _notifier;

        public EventsHandler(SettingsEntity settings, long vaultId, ITaskNotifier notifier)
        {
            InjectDepedencies(settings);
            var vaultInfo = ReturnVaultInfo(vaultId);
            _localRoot = vaultInfo.Path;
            _driveRoot = GetOrCreateRootFolder();
            _vaultId = vaultId;
            _notifier = notifier;
            DownloadAllFilesAsync(vaultInfo.Path);
        }

        private VaultsEntity ReturnVaultInfo(long vaultId)
        {
            var vaultRepo = new VaultsRepository();
            var vault = vaultRepo.GetById(vaultId)
                ?? throw new Exception($"Vault with ID {vaultId} not found.");

            return vault;
        }

        public async void DownloadAllFilesAsync(string vaultPath) =>
            await Task.Run(() => DownloadAllFiles(_driveRoot, vaultPath));

        #region Event Handlers

        public void ChangedEventHandler(object sender, FileSystemEventArgs e) =>
            HandleFileEvent(e.FullPath, TaskTypes.ChangeFile, UpdateFile);

        public void CreatedEventHandler(object sender, FileSystemEventArgs e) =>
            HandleFileEvent(e.FullPath, TaskTypes.CreateFile, UploadFile);

        public void DeletedEventHandler(object sender, FileSystemEventArgs e) =>
            HandleFileEvent(e.FullPath, TaskTypes.DeleteFile, DeleteFile);

        public void RenamedEventHandler(object sender, RenamedEventArgs e)
        {
            string fileName = e.FullPath;

            if (IsInternalFile(fileName)) return;

            var (isPrepared, taskId) = PrepareTask(fileName, TaskTypes.RenameFile);

            if (!WaitForFileReady(fileName) || !isPrepared)
            {
                EnqueueTask(fileName, TaskTypes.RenameFile, _localRoot, e.OldFullPath, e.FullPath, _driveRoot);
                return;
            }

            using var tc = new TaskWrapper(fileName);
            RenameFile(_localRoot, e.OldFullPath, e.FullPath, _driveRoot);
            tc.Complete();

            NotifyTask(taskId);
            Console.WriteLine($"[{timestamp}] RENOMEADO {e.OldName} -> {fileName}");
        }

        #endregion

        #region Helpers

        private async void HandleFileEvent(string fullPath, TaskTypes type, Func<string, string, string, string> action)
        {
            if (IsInternalFile(fullPath)) return;

            var (isPrepared, taskId) = PrepareTask(fullPath, type);

            if (!type.Equals(TaskTypes.DeleteFile) && File.Exists(fullPath))
            {
                if (!WaitForFileReady(fullPath) || !(await Helpers.HasFileSettledAsync(fullPath)) || !isPrepared)
                {
                    EnqueueTask(fullPath, type, _localRoot, "", "", _driveRoot);
                    return;
                }
            }

            if (!type.Equals(TaskTypes.DeleteFile) && File.Exists(fullPath))
            {
                byte[] newHash = GetFileHash(fullPath);

                if (recentHashes.TryGetValue(fullPath, out var oldHash) && oldHash.SequenceEqual(newHash))
                {
                    return;
                }

                recentHashes[fullPath] = newHash;
            }

            using var tc = new TaskWrapper(fullPath);

            if (!string.IsNullOrEmpty(action(_localRoot, fullPath, _driveRoot)))
            {
                tc.Complete();

                NotifyTask(taskId);

                Console.WriteLine($"[{timestamp}] {type.ToString().ToUpper()} {fullPath}");
            }

            Console.WriteLine($"[{timestamp}] {type.ToString().ToUpper()} {fullPath}");
        }

        private async void NotifyTask(long taskId)
        {
            try
            {
                var taskRepo = new TaskRepository();
                var taskEntity = taskRepo.GetById(taskId);

                await _notifier.NotifyAsync(taskEntity).ConfigureAwait(false);

            }
            catch (Exception ex)
            {
                throw new Exception($"Error notifying task: {ex}");
            }
        }


        private void EnqueueTask(string path, TaskTypes type, string localRoot, string oldPath, string newPath, string driveRoot)
        {
            if (!_taskQueueRepository.IsFileInQueue(path))
            {
                var task = new TaskQueue();
                task.CreateTaskInQueue(localRoot, oldPath, newPath, driveRoot, path, type);
                _taskQueueRepository.AddTaskQueue(task);

                Console.WriteLine($"File {Path.GetFileName(path)} queued (tmp or locked).");
            }
        }

        private static bool IsInternalFile(string name)
        {
            var isinternal = name.StartsWith("~") || name.EndsWith(".tmp") || name.EndsWith(".md~") ||
            name.EndsWith("sync.db") || name.EndsWith("sync.db-journal") || name.EndsWith("sync.db-wal") || name.Contains(".OBSIDIANTEST");

            return isinternal;
        }
        private static new bool IsFileLocked(string path)
        {
            if (!File.Exists(path)) return false;

            if (Helpers.IsFileInUse(path)) return true;

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

        private static byte[] GetFileHash(string fullPath)
        {
            try
            {
                Console.WriteLine("Comparing hash from file.");
                using var sha256 = SHA256.Create();
                using var stream = File.Open(fullPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
                return sha256.ComputeHash(stream);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private (bool isPrepared, long taskId) PrepareTask(string name, TaskTypes type)
        {
            var taskRepo = new TaskRepository();
            var task = taskRepo.GetByFileId(name) ?? new TaskEntity();

            if (task.Id == 0)
                task.CreateTask(name, type, _vaultId);

            var taskCreation = new TaskCreation(taskRepo);
            taskCreation.Data(task);

            var (isPrepared, taskId) = taskCreation.SaveTask();

            return (isPrepared, taskId);
        }

        private static bool WaitForFileReady(string path, int maxRetries = 10, int delayMs = 1000)
        {
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                if (!IsFileLocked(path))
                    return true;

                Thread.Sleep(delayMs);
            }
            return false;
        }

        #endregion
    }
}
