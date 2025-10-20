using G.Sync.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Repository
{
    public class TaskQueueRepository : ITaskQueueRepository
    {
        private readonly GSyncContext _context = new GSyncContext();

        public IEnumerable<TaskQueue> GetTaskQueues() => _context.TaskQueues.ToList();
        public void AddTaskQueue(TaskQueue taskQueue)
        {
            _context.TaskQueues.Add(taskQueue);
            _context.SaveChanges();
        }
    }
}
