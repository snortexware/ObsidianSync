using G.Sync.Common;
using G.Sync.DataContracts;
using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using G.Sync.Google.Api;
using G.Sync.Repository;
using G.Sync.TasksManagment;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using static G.Sync.Entities.TaskEntity;
using static G.Sync.Google.Api.FolderFileProcess;

namespace G.Sync.External.IO
{
    public class EventsHandler : FolderFileProcess, IEventsHandler
    {
        private static readonly ConcurrentDictionary<string, byte[]> recentHashes = new();
        private readonly string _localRoot;
        private readonly string _driveRoot;
        private readonly DateTime timestamp = DateTime.UtcNow;
        private readonly TaskQueueRepository _taskQueueRepository = new();

        public EventsHandler(SettingsEntity settings, string vaultPath)
        {
            InjectDepedencies(settings);
            _localRoot = vaultPath;
            _driveRoot = GetOrCreateRootFolder();
            DownloadAllFilesAsync(vaultPath);
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

            if (!WaitForFileReady(fileName))
            {
                EnqueueTask(fileName, TaskTypes.RenameFile, _localRoot, e.OldFullPath, e.FullPath, _driveRoot);
                return;
            }

            if (PrepareTask(fileName, TaskTypes.RenameFile))
            {
                using var tc = new TaskWrapper(fileName);
                RenameFile(_localRoot, e.OldFullPath, e.FullPath, _driveRoot);
                tc.Complete();
                Console.WriteLine($"[{timestamp}] RENOMEADO {e.OldName} -> {fileName}");
            }
        }

        #endregion

        #region Helpers

        private async void HandleFileEvent(string fullPath, TaskTypes type, Func<string, string, string, string> action)
        {
            if (IsInternalFile(fullPath)) return;

            if (!type.Equals(TaskTypes.DeleteFile) && File.Exists(fullPath))
            {
                if (!WaitForFileReady(fullPath) || !(await Helpers.HasFileSettledAsync(fullPath)))
                {
                    EnqueueTask(fullPath, type, _localRoot, "", "", _driveRoot);
                    return;
                }
            }

            if (PrepareTask(fullPath, type))
            {
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
                    Console.WriteLine($"[{timestamp}] {type.ToString().ToUpper()} {fullPath}");
                }
            }

            Console.WriteLine($"[{timestamp}] {type.ToString().ToUpper()} {fullPath}");
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
            name.EndsWith("sync.db") || name.EndsWith("sync.db-journal") || name.EndsWith("sync.db-wal");

            return isinternal;
        }
        private static bool IsFileLocked(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            if (Helpers.IsFileInUse(path))
                return true;

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



        private bool PrepareTask(string name, TaskTypes type)
        {
            var taskRepo = new TaskRepository();
            var task = taskRepo.GetByFileId(name) ?? new TaskEntity();

            if (task.Id == 0)
                task.CreateTask(name, type);

            var taskCreation = new TaskCreation(taskRepo);
            taskCreation.Data(task);
            taskCreation.SaveTask();

            Console.WriteLine($"Task prepared with ID: {task.Id}");
            return taskCreation.IsPrepared;
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
