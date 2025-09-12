using G.Sync.DataContracts;
using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
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
            _localRoot = settings.GoogleDriveFolderName; // fallback
            _driveRoot = GetOrCreateRootFolder(settings);

            DownloadAllFiles(_driveRoot);
        }

        public void ChangedEventHandler(object sender, FileSystemEventArgs e)
        {
            if (PrepareTask(e.Name, TaskTypes.ChangeFile))
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
                    UpdateFile(_localRoot, e.FullPath, _driveRoot);
                    tc.Complete();
                    Console.WriteLine($"[{timestamp}] MODIFICADO {e.Name}");
                }
            }
        }

        public void CreatedEventHandler(object sender, FileSystemEventArgs e)
        {
            if (PrepareTask(e.Name, TaskTypes.CreateFile))
            {
                byte[] hash = GetFileHash(e.FullPath);
                recentHashes[e.FullPath] = hash;

                using (var tc = new TaskWrapper(e.Name))
                {
                    UploadFile(e.FullPath, _localRoot, _driveRoot);
                    tc.Complete();
                    Console.WriteLine($"[{timestamp}] CRIADO {e.Name}");
                }
            }
        }

        public void DeletedEventHandler(object sender, FileSystemEventArgs e)
        {
            if (!IsInternalFile(e.Name) && PrepareTask(e.Name, TaskTypes.DeleteFile))
            {
                using (var tc = new TaskWrapper(e.Name))
                {
                    DeleteFile(e.FullPath, _localRoot, _driveRoot);
                    tc.Complete();
                    Console.WriteLine($"[{timestamp}] REMOVIDO {e.Name}");
                }
            }
        }

        public void RenamedEventHandler(object sender, RenamedEventArgs e)
        {
            if (!IsInternalFile(e.Name) && PrepareTask(e.Name, TaskTypes.RenameFile))
            {
                using (var tc = new TaskWrapper(e.Name))
                {
                    RenameFile(e.OldFullPath, e.FullPath, _localRoot, _driveRoot);
                    tc.Complete();
                    Console.WriteLine($"[{timestamp}] RENOMEADO {e.OldName} -> {e.Name}");
                }
            }
        }

        private static bool PrepareTask(string name, TaskTypes type)
        {
            var task = new TaskEntity();
            task.CreateTask(name, type);
            var taskRepo = new TaskRepository();
            var taskCreation = new TaskCreation(taskRepo);
            taskCreation.CreateTask();
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
    }
}
