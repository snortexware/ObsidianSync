using Database.Connection;
using G.Sync.VersionSystem.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using G.Sync.Service;
using G.Sync.Repository;

namespace G.Sync.VersionSystem.Default
{
    public class DefaultTablesCreation : IVersion
    {
        public DateTime Date => new(2025, 08, 23);

        public void Run()
        {
            var settingsRepo = new SettingsRepository();
            settingsRepo.CreateSettingsTable();
            settingsRepo.CreateDefaultSettings();
        }
    }
}
