using G.Sync.Common;
using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using G.Sync.Repository;
using G.Sync.Service.Interfaces;
using G.Sync.TasksManagment;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using static G.Sync.Entities.TaskEntity;

namespace G.Sync.External.IO
{
    public class EventsHandler : IEventsHandler
    {
        private static readonly ConcurrentDictionary<string, byte[]> recentHashes = new();

        private string _localRoot;
        private string _driveRoot;
        private long _vaultId;
        private bool _initialized;
        private readonly SemaphoreSlim _initLock = new(1, 1);

        private readonly ITaskQueueRepository _taskQueueRepository;
        private readonly IVaultsRepository _vaultsRepository;
        private readonly ITaskNotifier _notifier;
        private readonly IFolderFileProcess _folder;

        public EventsHandler(
            IVaultsRepository vaultsRepository,
            ITaskQueueRepository taskQueueRepository,
            IFolderFileProcess folderFileProcess,
            ITaskNotifier notifier)
        {
            _vaultsRepository = vaultsRepository;
            _taskQueueRepository = taskQueueRepository;
            _folder = folderFileProcess;
            _notifier = notifier;
        }

        public async Task InitializeAsync(long vaultId, ISettingsEntity settings)
        {
            _folder.Settings = settings;

            if (_initialized) return;

            await _initLock.WaitAsync();
            try
            {
                if (_initialized) return;

                _vaultId = vaultId;

                var vault = _vaultsRepository.GetById(vaultId) 
                    ?? throw new Exception($"Vault {vaultId} not found");

                _localRoot = vault.Path;

                _driveRoot = await _folder.GetOrCreateRootFolderAsync();

                await DownloadAllFilesAsync(vault.Path);

                _initialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        public async Task DownloadAllFilesAsync(string vaultPath)
        {
            await _folder.DownloadAllFilesAsync(_driveRoot, vaultPath);
        }

        #region Event Handlers

        public void ChangedEventHandler(object sender, FileSystemEventArgs e) =>
            _ = HandleFileEvent(e.FullPath, TaskTypes.ChangeFile, _folder.UpdateFileAsync);

        public void CreatedEventHandler(object sender, FileSystemEventArgs e) =>
            _ = HandleFileEvent(e.FullPath, TaskTypes.CreateFile, _folder.UploadFileAsync);

        public void DeletedEventHandler(object sender, FileSystemEventArgs e) =>
            _ = HandleFileEvent(e.FullPath, TaskTypes.DeleteFile, _folder.DeleteFileAsync);

        public void RenamedEventHandler(object sender, RenamedEventArgs e) =>
            _ = HandleRenameEvent(e);

        #endregion

        #region Core Logic

        private async Task HandleFileEvent(
            string path,
            TaskTypes type,
            Func<string, string, string, Task<string>> action)
        {
            if (IsInternalFile(path)) return;

            var (isPrepared, taskId) = PrepareTask(path, type);

            if (type != TaskTypes.DeleteFile && File.Exists(path))
            {
                if (!WaitForFileReady(path) ||
                    !(await Helpers.HasFileSettledAsync(path)) ||
                    !isPrepared)
                {
                    Enqueue(path, type, "", "");
                    return;
                }
            }

            if (type != TaskTypes.DeleteFile && File.Exists(path))
            {
                var newHash = GetFileHash(path);

                if (recentHashes.TryGetValue(path, out var oldHash) &&
                    oldHash.SequenceEqual(newHash))
                    return;

                recentHashes[path] = newHash;
            }

            using var tc = new TaskWrapper(path);

            var result = await action(_localRoot, path, _driveRoot);

            if (!string.IsNullOrEmpty(result))
            {
                tc.Complete();
                await NotifyTask(taskId);

                Log(type, path);
            }
        }

        private async Task HandleRenameEvent(RenamedEventArgs e)
        {
            var path = e.FullPath;

            if (IsInternalFile(path)) return;

            var (isPrepared, taskId) = PrepareTask(path, TaskTypes.RenameFile);

            if (!WaitForFileReady(path) || !isPrepared)
            {
                Enqueue(path, TaskTypes.RenameFile, e.OldFullPath, e.FullPath);
                return;
            }

            using var tc = new TaskWrapper(path);

            var result = await _folder.RenameFileAsync(
                _localRoot,
                e.OldFullPath,
                e.FullPath,
                _driveRoot);

            if (!string.IsNullOrEmpty(result))
            {
                tc.Complete();
                await NotifyTask(taskId);

                Console.WriteLine($"RENAMED {e.OldName} -> {path}");
            }
        }

        #endregion

        #region Helpers

        private async Task NotifyTask(long taskId)
        {
            try
            {
                var repo = new TaskRepository();
                var task = repo.GetById(taskId);

                await _notifier.NotifyAsync(task);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Notify error: {ex.Message}");
            }
        }

        private void Enqueue(string path, TaskTypes type, string oldPath, string newPath)
        {
            if (_taskQueueRepository.IsFileInQueue(path)) return;

            var task = new TaskQueue();

            Console.WriteLine($"Before creating the task LocalRoot = {_localRoot}, DriveRoot = {_driveRoot}");

            task.CreateTaskInQueue(_localRoot, oldPath, newPath, _driveRoot, path, type);

            _taskQueueRepository.AddTaskQueue(task);

            Console.WriteLine($"Queued: {Path.GetFileName(path)}");
        }

        private static void Log(TaskTypes type, string path)
        {
            Console.WriteLine($"{type.ToString().ToUpper()} {path}");
        }

        private static bool IsInternalFile(string name)
        {
            return name.StartsWith("~") ||
                   name.EndsWith(".tmp") ||
                   name.EndsWith(".md~") ||
                   name.Contains("sync.db") ||
                   name.Contains(".OBSIDIANTEST") ||
                   name.Contains(".obsidian");
        }

        private static bool IsFileLocked(string path)
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

        private static byte[] GetFileHash(string path)
        {
            using var sha = SHA256.Create();
            using var stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            return sha.ComputeHash(stream);
        }

        private (bool, long) PrepareTask(string name, TaskTypes type)
        {
            var repo = new TaskRepository();
            var task = repo.GetByFileId(name) ?? new TaskEntity();

            if (task.Id == 0)
                task.CreateTask(name, type, _vaultId);

            var creator = new TaskCreation(repo);
            creator.Data(task);

            return creator.SaveTask();
        }

        private static bool WaitForFileReady(string path, int retries = 10, int delay = 500)
        {
            for (int i = 0; i < retries; i++)
            {
                if (!IsFileLocked(path))
                {
                    return true;
                }

                Thread.Sleep(delay);
            }

            return false;
        }

        #endregion
    }
}