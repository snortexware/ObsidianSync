using G.Sync.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Repository
{
    public class TaskQueueRepository : ITaskQueueRepository
    {
        public void RemoveTaskQueue(long id)
        {
            using var context = new GSyncContext();
            var taskQueue = context.TaskQueues.FirstOrDefault(tq => tq.Id == id);

            if (taskQueue != null)
            {
                Console.WriteLine("Removing TaskQueue with ID: " + id);
                context.TaskQueues.Remove(taskQueue);
                context.SaveChanges();
            }
        }

        public bool IsFileInQueue(string path)
        {
            using var context = new GSyncContext();
            return context.TaskQueues.Any(tq => tq.FilePath == path);
        }

        public IEnumerable<TaskQueue> GetTaskQueues()
        {
            try
            {
                var context = new GSyncContext();
                return context.TaskQueues.AsNoTracking();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve task queues from the database.", ex);
            }
        }
        public void AddTaskQueue(TaskQueue taskQueue)
        {
            using var context = new GSyncContext();
            context.TaskQueues.Add(taskQueue);
            context.SaveChanges();
        }
    }
}
