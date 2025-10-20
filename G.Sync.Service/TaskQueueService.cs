using G.Sync.Google.Api;
using G.Sync.Repository;
using G.Sync.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Service
{
    public class TaskQueueService
    {
        private readonly ITaskQueueRepository _repository;
        private readonly IFolderFileService _folderService;

        public TaskQueueService(ITaskQueueRepository repository, IFolderFileService folderService)
        {
            _repository = repository;
            _folderService = folderService;
        }

        public void ProcessTaskQueues()
        {
            Console.WriteLine("Starting to process task queues...");

            var taskQueues = _repository.GetTaskQueues();

            if (!taskQueues.Any())
            {
                Console.WriteLine("No task queues to process.");
                return;
            }

            foreach (var taskQueue in taskQueues)
            {
                switch (taskQueue.Type)
                {
                    case Entities.TaskEntity.TaskTypes.UploadFile:
                        _folderService.UpdateFile(taskQueue.LocalRoot, taskQueue.FilePath, taskQueue.DriveRoot);
                        break;
                    case Entities.TaskEntity.TaskTypes.DeleteFile:
                        Console.WriteLine("Its delete.");
                        _folderService.DeleteFile(taskQueue.LocalRoot, taskQueue.FilePath, taskQueue.DriveRoot);
                        break;
                    case Entities.TaskEntity.TaskTypes.RenameFile:
                        _folderService.RenameFile(taskQueue.LocalRoot, taskQueue.OldPath, taskQueue.NewPath, taskQueue.DriveRoot);
                        break;
                    default:
                        Console.WriteLine($"Unknown task type for Task ID: {taskQueue.Id}");
                        break;
                }

                Console.WriteLine($"Processing TaskQueue for Task ID: {taskQueue.Id}");
            }
        }
    }
}
