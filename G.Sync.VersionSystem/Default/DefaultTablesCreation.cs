using Database.Connection;
using G.Sync.VersionSystem.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using G.Sync.Settings;

namespace G.Sync.VersionSystem.Default
{
    public class DefaultTablesCreation : IVersion
    {
        #region Consts
        private readonly string _taskTableSql = @"CREATE TABLE IF NOT EXISTS TASKS (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            NAME TEXT NOT NULL, EXECUTETS TEXT NOT NULL,
            COMPLETED INTEGER DEFAULT 0, FILEID TEXTO NOT NULL,
            TASKTYPE INTEGER";

        private readonly string _insertDefaultSettingsSql =
                        @"INSERT INTO SETTINGS (FOLDER) VALUES (@folder)";

        private readonly string _settingsTableSql =
            @"CREATE TABLE IF NOT EXISTS SETTINGS (FOLDER TEXT NOT NULL)";

        #endregion
        public DateTime Date => new(2025, 08, 23);

        public void Run()
        {
            var cnn = GlobalDbConnection.Instance.Connection;
            cnn.Execute(_taskTableSql);
            cnn.Execute(_settingsTableSql);

            cnn.Execute(_insertDefaultSettingsSql,
                new { folder = SystemSettingsContext.DefaultFolder });
        }
    }
}
