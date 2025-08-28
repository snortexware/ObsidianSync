using G.Sync.Entities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using G.Sync.Entities;
using G.Sync.DatabaseManagment;
using Dapper;

namespace G.Sync.Repository
{
    public class TaskRepository : ITaskRepository
    {
        private Entities.Task _task { get; set; }
        public Entities.Task Create()
        {
            _task = new Entities.Task();

            return _task;
        }

        public Entities.Task Get(string where)
        {
            var _connection = DataBaseContext.Instance.GetConnection();

            var results = _connection.QueryFirstOrDefault<Entities.Task>(where);

            return results;
        }

        public static Entities.Task Get(int id)
        {
            var _connection = DataBaseContext.Instance.GetConnection();

            var results = _connection.QueryFirstOrDefault<Entities.Task>("WHERE ID = @id", new { id = id});

            return results ?? null;
        }

        public void Save()
        {
            var teste = new EntityRepository<Entities.Task>();

            teste.Create();
            teste.Entity.

            
            var _connection = DataBaseContext.Instance.GetConnection();

            _connection.Execute("INSERT INTO TASKS()",
                new { @ })

                        public int Id { get; set; }
        public string Name { get; set; }
        public string ExecuteTs { get; set; }
        public int Status { get; set; }
        public string FileId { get; set; }
        public int TaskType { get; set; }

    }

    }
}
