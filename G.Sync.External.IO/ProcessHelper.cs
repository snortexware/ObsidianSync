using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using G.Sync.Repository;
using System.IO;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Xml.Linq;
using static G.Sync.Entities.TaskEntity;

namespace G.Sync.External.IO
{
    public class ProcessHelper
    {
        private readonly ITaskNotifier _notifier;
        public ProcessHelper(ITaskNotifier notifier)
        {
            _notifier = notifier;
        }

        private static readonly string[] InternalFiles = new[] { "sync.db", "sync.db-journal" };
        public static bool IsInternalFile(string name)
        {
            foreach (var internalFile in InternalFiles)
            {
                if (name.Contains(internalFile))
                    return true;
            }
            return false;
        }

        public static byte[] GetFileHash(string path)
        {
            try
            {
                using var fs = File.OpenRead(path);
                using var sha = SHA256.Create();
                return sha.ComputeHash(fs);
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }

        public static TaskEntity InitialTaskProcess(string fileId, TaskTypes taskType)
        {
            var task = new TaskEntity();
            return task.CreateTask(new FileId(fileId), taskType);
        }

        public bool PrepareTask(string name, string path, TaskTypes taskType)
        {
            if (File.Exists(path) && !IsInternalFile(name))
            {
                var task = InitialTaskProcess(name, taskType);

                try
                {
                    var taskRepo = new TaskRepository();
                    taskRepo.Create();
                    taskRepo.Save(task);

                    _notifier.NotifyAsync(task).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error registering task: {ex}");
                }

                return true;
            }

            return false;
        }
    }
}
