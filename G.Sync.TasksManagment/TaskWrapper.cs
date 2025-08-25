using Database.Connection;
using System.Numerics;
using Dapper;
using G.Sync.DataContracts;

namespace G.Sync.TasksManagment
{
    public class TaskWrapper(string fileId) : IDisposable
    {
        private readonly string _fileId = fileId;

        private bool _completed = false;

        private const string _updateTaskStatusSql =
            "UPDATE TASKS SET STATUS = @status WHERE FILEID = @ID AND (STATUS <> @status OR STATUS IS NULL)";

        public void Complete()
        {
            _completed = true;
        }

        public void Dispose()
        {
            var connection = GlobalDbConnection.Instance.Connection;
            var parameters = new DynamicParameters();
            parameters.Add("ID", _fileId);

            try
            {
                if (_completed)
                {
                    parameters.Add("status", StatusType.completed);
                    connection.Execute(_updateTaskStatusSql, parameters);
                }
            }
            catch (Exception ex)
            {
                parameters.Add("status", StatusType.failed);
                connection.Execute(_updateTaskStatusSql, parameters);
                throw new Exception("Failed to update task status to completed.", ex);
            }
        }
    }
}
