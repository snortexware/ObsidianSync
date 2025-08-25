using System.Data;
using System.IO;
using Microsoft.Data.Sqlite;
using Dapper;

namespace G.Sync.DatabaseManagment
{
    public sealed class DataBaseContext
    {
        private static readonly Lazy<DataBaseContext> _instance =
            new(() => new DataBaseContext());

        private readonly string _connectionString;

        private DataBaseContext()
        {
            var dbFolder = Path.GetDirectoryName(SettingsContext);

            if (!Directory.Exists(dbFolder))
                Directory.CreateDirectory(dbFolder);

            _connectionString = $"Data Source={dbPath};Pooling=true;";
        }

        /// <summary>
        /// Instância única do gerenciador de conexão.
        /// </summary>
        public static DataBaseContext Instance => _instance.Value;

        /// <summary>
        /// Retorna uma nova conexão aberta para o SQLite.
        /// O chamador deve usar "using" para garantir o Dispose().
        /// </summary>
        public IDbConnection GetConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open(); // opcional (Dapper abre automaticamente se precisar)
            return connection;
        }
    }
}
