using G.Sync.DataContracts;
using G.Sync.Google.Api;
using Google.Apis.Drive.v3;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using static G.Sync.Google.Api.ApiContext;
using static G.Sync.Google.Api.FolderFileProcess;

namespace MainEvents
{
    public class EventsHandler : FolderFileProcess
    {
        private static readonly ConcurrentDictionary<string, byte[]> recentHashes = new ConcurrentDictionary<string, byte[]>();
        private const string _localRoot = @"C:\obsidian-sync";
        private readonly DriveService _drive = Instance.Connection;
        private readonly string _driveRoot;
        private readonly DateTime timestamp = DateTime.UtcNow;

        public EventsHandler()
        {
            _driveRoot = GetOrCreateRootFolder(null); // Assuming settings are not needed here
            DownloadAllFiles(_drive, _driveRoot, _localRoot).Wait();
        }

        public void ChangedEventHandler(FileSystemEventArgs e)
        {
            if (PrepareTask(e.Name, e.FullPath))
            {
                byte[] newHash = GetFileHash(e.FullPath);
                if (recentHashes.TryGetValue(e.FullPath, out byte[] oldHash))
                {
                    if (oldHash.SequenceEqual(newHash))
                    {
                        return;
                    }
                }
                recentHashes[e.FullPath] = newHash;

                using (var tc = new TaskWrapper(e.Name))
                {
                    UpdateFile(e.FullPath, _localRoot, _driveRoot);
                    tc.Complete();
                    Console.WriteLine($"[{timestamp}] MODIFICADO {e.Name}");
                }
            }
        }

        public void CreatedEventHandler(FileSystemEventArgs e)
        {
            if (PrepareTask(e.Name, e.FullPath))
            {
                byte[] hash = GetFileHash(e.FullPath);
                recentHashes[e.FullPath] = hash;
                using (var tc = new TaskWrapper(e.Name))
                {
                    _fileProcessor.UploadFile(new ApiPathDto { FullPath = e.FullPath, LocalRoot = _localRoot, DriveRoot = _driveRoot });
                    tc.Complete();
                    Console.WriteLine($"[{timestamp}] CRIADO {e.Name}");
                }
            }
        }

        public void DeletedEventHandler(FileSystemEventArgs e)
        {
            if (!isInternalFile(e.Name) && PrepareTask(e.Name, e.FullPath))
            {
                using (var tc = new TaskWrapper(e.Name))
                {
                    _fileProcessor.DeleteFile(e.FullPath, _localRoot, _driveRoot);
                    tc.Complete();
                    Console.WriteLine($"[{timestamp}] REMOVIDO {e.Name}");
                }
            }
        }

        public void RenamedEventHandler(RenamedEventArgs e)
        {
            if (!isInternalFile(e.Name) && PrepareTask(e.Name, e.FullPath))
            {
                using (var tc = new TaskWrapper(e.Name))
                {
                    RenameFile(e.OldFullPath, e.FullPath, _localRoot, _driveRoot);
                    tc.Complete();
                    Console.WriteLine($"[{timestamp}] RENOMEADO {e.OldName} -> {e.Name}");
                }
            }
        }

        private static bool PrepareTask(string name, string fullPath)
        {
            return TaskCreation.PrepareTask.CreateTask(name, fullPath);
        }

        private static byte[] GetFileHash(string fullPath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(fullPath))
                {
                    return sha256.ComputeHash(stream);
                }
            }
        }

        private static bool isInternalFile(string name)
        {
            return name.StartsWith("~");
        }
    }
}