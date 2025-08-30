using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using G.Sync.Repository;
using static System.Net.Mime.MediaTypeNames;

namespace G.Sync.Repository
{
    public class SettingsRepository : EntityRepository<SettingsEntity>, ISettingsRepository
    {
        #region Consts
        private readonly string _createSettingsTableSql =
            @"CREATE TABLE IF NOT EXISTS SETTINGS (ID INTEGER PRIMARY KEY AUTOINCREMENT, FOLDER TEXT NOT NULL)";
        #endregion

        public void CreateDefaultSettings()
        {
            var settings = new SettingsEntity
            {
                Id = 1,
                Folder = @"C:\obsidian-sync"
            };

            Save(settings);
        }
              #region Consts

        #endregion
        public void CreateSettingsTable() => CreateEntityTable(_createSettingsTableSql);

        public SettingsEntity? GetSettings() => Get(1);

        public void SaveSettings(SettingsEntity? settings) => Save(settings);
    }
}