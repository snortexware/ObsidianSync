using Database.Connection;
using Dapper;
using G.Sync.DataContracts;
using System.Text.Json;

namespace G.Sync.Settings
{
    public static class SystemSettingsContext
    {
        #region Consts

        #endregion

        public static void SetSettings(string json)
        {
            var settingsObj = JsonSerializer.Deserialize<SettingsDto>(json);

            if(settingsObj is null)
                throw new ArgumentNullException(nameof(settingsObj));

            UpdateSettings(settingsObj.Folder);
        }

        public static void UpdateSettings(string folder)
        {
            var cnn = GlobalDbConnection.Instance.Connection;

            var results = cnn.Query<SettingsDto>(_settingsTableSql).FirstOrDefault() 
                ?? throw new Exception("Settings not found in database.");

            if (folder == results.Folder) return;

            if (string.IsNullOrEmpty(folder))
            {
                cnn.Execute(_settingsUpdateFolderSql, new { folder = DefaultFolder});
                return;
            }

            cnn.Execute(_settingsUpdateFolderSql, new { folder = folder});
        }
    }
}
