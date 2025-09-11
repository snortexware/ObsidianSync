using G.Sync.DataContracts;
using G.Sync.Entities;
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

namespace MainEvents
{
    public class EventsHandler : FolderFileProcess
    {
        private static readonly ConcurrentDictionary<string, byte[]> recentHashes = new ConcurrentDictionary<string, byte[]>();

        private readonly string _localRoot;
        private readonly DriveService _drive = Instance.Connection;
        private readonly string _driveRoot;
        private readonly DateTime timestamp = DateTime.UtcNow;

        public EventsHandler()
        {
            var settingsRepo = new SettingsRepository();
            var settings = settingsRepo.GetSettings();

            _localRoot = settings.GoogleDriveFolderName ?? @"C:\obsidian-sync"; // fallback
            _driveRoot = GetOrCreateRootFolder(settings);

            DownloadAllFiles(_driveRoot);
        }

        public void ChangedEventHandler(FileSystemEventArgs e)
        {
            if (PrepareTask(e.Name))
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

        public void CreatedEventHandler(FileSystemEventArgs e)
        {
            if (PrepareTask(e.Name))
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

        public void DeletedEventHandler(FileSystemEventArgs e)
        {
            if (!IsInternalFile(e.Name) && PrepareTask(e.Name))
            {
                using (var tc = new TaskWrapper(e.Name))
                {
                    DeleteFile(e.FullPath, _localRoot, _driveRoot);
                    tc.Complete();
                    Console.WriteLine($"[{timestamp}] REMOVIDO {e.Name}");
                }
            }
        }

        public void RenamedEventHandler(RenamedEventArgs e)
        {
            if (!IsInternalFile(e.Name) && PrepareTask(e.Name))
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
            return name.StartsWith("~");
        }
    }
}
