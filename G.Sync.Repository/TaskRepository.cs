using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Repository
{
    public class TaskRepository : EntityRepository<TaskEntity>, ITaskRepository
    {
        public TaskEntity? GetById(int id) => Get(id);
        public TaskEntity? GetFirstOrDefault(string sql, object parameters) => GetFirstOrDefault(sql, parameters);
        public new void Save(TaskEntity entity) => Save(entity);
    }
}
