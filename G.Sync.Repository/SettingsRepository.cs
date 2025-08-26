using G.Sync.Entities;
using G.Sync.Service;
using Microsoft.Data.Sqlite;
using Dapper;

namespace G.Sync.Repository
{
    public class SettingsRepository(SqliteConnection connection) : ISettingsRepository
    {
        private readonly SqliteConnection _connection = connection;
        public Settings Get()
        {
            var folder = _connection.QuerySingleOrDefault<string>(
            "SELECT FOLDER FROM SETTINGS");

            return folder is null ? throw new Exception("Settings not found.") : new Settings(folder);
        }

        public void Save(Settings settings)
        {
            _connection.Execute(
                "UPDATE SETTINGS SET FOLDER = @folder",
                new { folder = settings.Folder }
            );
        }
    }
}

