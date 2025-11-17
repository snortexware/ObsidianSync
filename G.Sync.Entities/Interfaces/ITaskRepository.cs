using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Entities.Interfaces
{
    public interface ITaskRepository
    {
        public TaskEntity? GetByFileId(string fileId);
        TaskEntity? GetById(long id);
        long Save(TaskEntity entity);
        public List<TaskEntity> GetPendingTasks();
    }
}
