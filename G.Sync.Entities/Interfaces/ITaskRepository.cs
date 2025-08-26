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
        void Create();
        TaskEntity? GetById(string id);
        TaskEntity? GetFirstOrDefault(string sql, object parameters);
        void Save(TaskEntity entity);
    }
}
