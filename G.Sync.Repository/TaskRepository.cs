using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static G.Sync.Entities.TaskEntity;

namespace G.Sync.Repository
{
    public class TaskRepository : ITaskRepository
    {
        public TaskEntity? GetByFileId(string fileId)
        {
            var dbContext = new GSyncContext();
            return dbContext.Tasks.FirstOrDefault(t => t.FileId == fileId);
        }

        public TaskEntity? GetById(long id)
        {
            var dbContext = new GSyncContext();
            return dbContext.Tasks.FirstOrDefault(x => x.Id == id);
        }

        public List<TaskEntity> GetPendingTasks()
        {
            var dbContext = new GSyncContext();

            return dbContext.Tasks.Where(x => x.Status == TasksStatus.Pending).ToList();
        }


        public void Save(TaskEntity entity)
        {
            var dbContext = new GSyncContext();

            var existingTask = dbContext.Tasks.FirstOrDefault(x => x.FileId == entity.FileId);

            if (existingTask == null)
            {
                dbContext.Tasks.Add(entity);
            }
            else
            {
                existingTask.CopyFrom(entity);
            }

            dbContext.SaveChanges();
        }
    }
}
