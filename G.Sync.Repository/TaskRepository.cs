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
    public class TaskRepository : EntityRepository<TaskEntity>, ITaskRepository
    {
        #region Consts

        private readonly string _sqlCreateTaskTable = @"CREATE TABLE IF NOT EXISTS TASKS (
            ID INTEGER PRIMARY KEY AUTOINCREMENT,
            NAME TEXT NOT NULL, EXECUTETS TEXT NOT NULL,
            STATUS INTEGER DEFAULT 0, FILEID TEXTO NOT NULL,
            TASKTYPE INTEGER";
        #endregion
        public TaskEntity? GetByFileId(string fileId)
        {
            var parameters = new { ID = fileId, completed = TasksStatus.Completed };
            const string sql = "SELECT * FROM TASKS WHERE FILEID = @ID AND STATUS <> @completed";
            return GetFirstOrDefault(sql, parameters);
        }

        public void CreateTaskTable() => CreateEntityTable(_sqlCreateTaskTable);

        public TaskEntity? GetById(string id) => Get(id);
        public TaskEntity? GetFirstOrDefault(string sql, object parameters) => GetFirstOrDefault(sql, parameters);
        public new void Save(TaskEntity entity) => Save(entity);
    }
}
