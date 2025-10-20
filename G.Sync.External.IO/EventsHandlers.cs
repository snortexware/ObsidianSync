using G.Sync.DataContracts;
using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using G.Sync.External.IO.Quartz;
using G.Sync.Google.Api;
using G.Sync.Repository;
using G.Sync.TasksManagment;
using Google.Apis.Drive.v3;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using static G.Sync.Entities.TaskEntity;
using static G.Sync.Google.Api.ApiContext;
using static G.Sync.Google.Api.FolderFileProcess;

namespace G.Sync.External.IO
{
    public class EventsHandler : FolderFileProcess, IEventsHandler
    {
        private static readonly ConcurrentDictionary<string, byte[]> recentHashes = new ConcurrentDictionary<string, byte[]>();
        private readonly string _localRoot;
        private readonly string _driveRoot;
        private readonly DateTime timestamp = DateTime.UtcNow;

        public EventsHandler(SettingsEntity settings)
        {
            InjectDepedencies(settings);

            _localRoot = settings.GoogleDriveFolderName; // fallback
            _driveRoot = GetOrCreateRootFolder();
            DownloadAllFiles(_driveRoot);
        }

        public void ChangedEventHandler(object sender, FileSystemEventArgs e)
        {
            if (IsInternalFile(e.Name)) return;

            if (!IsFileLocked(e.FullPath) && PrepareTask(e.Name, TaskTypes.ChangeFile))
            {
                byte[] newHash = GetFileHash(e.FullPath);

                if (recentHashes.TryGetValue(e.FullPath, out byte[] oldHash))
                {
                    if (oldHash.SequenceEqual(newHash))
                        return;
                }

                recentHashes[e.FullPath] = newHash;

                using (var tc = new TaskWrapper(e.Name))
                {
                    if (!string.IsNullOrEmpty(UpdateFile(_localRoot, e.FullPath, _driveRoot)))
                    {
                        tc.Complete();
                        Console.WriteLine($"[{timestamp}] MODIFICADO {e.Name}");
                    }
                    else
                    {
                        Console.WriteLine($"[{timestamp}] ARQUIVO {e.Name} TRAVADO, AGUARDANDO");
                    }
                }
            }
        }

        public void CreatedEventHandler(object sender, FileSystemEventArgs e)
        {
            if (IsInternalFile(e.Name)) return;

            if (!IsFileLocked(e.FullPath) && PrepareTask(e.Name, TaskTypes.CreateFile))
            {
                byte[] hash = GetFileHash(e.FullPath);
                recentHashes[e.FullPath] = hash;

                using (var tc = new TaskWrapper(e.Name))
                {
                    UploadFile(_localRoot, e.FullPath,  _driveRoot);
                    tc.Complete();
                    Console.WriteLine($"[{timestamp}] CRIADO {e.Name}");
                }
            }
        }

        public void DeletedEventHandler(object sender, FileSystemEventArgs e)
        {
            if (IsInternalFile(e.Name)) return;

            var ready = IsFileReady(_localRoot, "", "", _driveRoot, e.FullPath, TaskTypes.DeleteFile);

            if (!ready) return;

            if (PrepareTask(e.Name, TaskTypes.DeleteFile))
            {
                using (var tc = new TaskWrapper(e.Name))
                {
                    DeleteFile(_localRoot, e.FullPath, _driveRoot);
                    tc.Complete();
                    Console.WriteLine($"[{timestamp}] REMOVIDO {e.Name}");
                }
            }
        }

        public void RenamedEventHandler(object sender, RenamedEventArgs e)
        {
            if (IsInternalFile(e.Name)) return;

            var ready = IsFileReady(_localRoot, e.OldFullPath, e.FullPath, _driveRoot, e.FullPath, TaskTypes.RenameFile);

            if(!ready) return;  

            if (PrepareTask(e.Name, TaskTypes.RenameFile))
            {
                using (var tc = new TaskWrapper(e.Name))
                {
                    RenameFile(_localRoot, e.OldFullPath, e.FullPath, _driveRoot);
                    tc.Complete();
                    Console.WriteLine($"[{timestamp}] RENOMEADO {e.OldName} -> {e.Name}");
                }
            }
        }

        private static bool PrepareTask(string name, TaskTypes type)
        {
            var taskRepo = new TaskRepository();
            var task = taskRepo.GetByFileId(name);

            if (task == null)
            {
                task = new TaskEntity();
                task.CreateTask(name, type);
            }

            var taskCreation = new TaskCreation(taskRepo);
            taskCreation.Data(task);
            taskCreation.SaveTask();

            Console.WriteLine("the id is " + task.Id);
            return taskCreation.IsPrepared;
        }

        private static byte[] GetFileHash(string fullPath)
        {
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(fullPath))
            {
                return sha256.ComputeHash(stream);
            }
        }

        private static bool IsInternalFile(string name)
        {
            return name.StartsWith("~") || name.EndsWith(".tmp") || name.Equals("sync.db") || name.Equals("sync.db-journal");
        }
        public static bool IsFileLocked(string path, int maxRetries = 10, int delayBetweenRetriesMs = 1500)
        {
            if (!File.Exists(path))
                return false;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    // Double-check that file is still accessible
                    if (stream.Length >= 0)
                    {
                        if (attempt > 1)
                            Console.WriteLine($"File {Path.GetFileName(path)} unlocked after {attempt} attempts.");

                        return false; // not locked
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine($"File {Path.GetFileName(path)} locked (attempt {attempt}/{maxRetries})...");
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine($"Unauthorized access to file {Path.GetFileName(path)} (attempt {attempt}/{maxRetries})...");
                }

                Thread.Sleep(delayBetweenRetriesMs);
            }

            Console.WriteLine($"File {Path.GetFileName(path)} is still locked after {maxRetries} retries.");
            return true;
        }
        private bool IsFileReady(
     string localRoot = "",
     string oldPath = "",
     string newPath = "",
     string driveRoot = "",
     string filePath = "",
     TaskTypes taskType = TaskTypes.DownloadFile)
        {
            Console.WriteLine($"IsFileReady - Checking lock status for: {filePath}");
            Console.WriteLine($"Task type: {taskType}");

            if (!IsFileLocked(filePath))
            {
                Thread.Sleep(200);
                if (!IsFileLocked(filePath))
                {
                    Console.WriteLine($"File {Path.GetFileName(filePath)} is ready.");
                    return true;
                }
            }

            var taskRepo = new TaskQueueRepository();
            var task = new TaskQueue();
            task.CreateTaskInQueue(localRoot, oldPath, newPath, driveRoot, filePath, taskType);
            taskRepo.AddTaskQueue(task);

            Console.WriteLine($"File {Path.GetFileName(filePath)} still locked after multiple retries → sent to queue.");
            return false;
        }
    }
} 
