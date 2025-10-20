﻿using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using G.Sync.Repository;
using static System.Net.Mime.MediaTypeNames;

namespace G.Sync.Repository
{
    public class SettingsRepository : ISettingsRepository
    {
        public void CreateDefaultSettings()
        {
            var dbContext = new GSyncContext();
            var settings = new SettingsEntity();

            settings.CreateSettings("MyDriveProject", "GDriveFolder", "C:\\obsidian-sync");
            dbContext.Settings.Add(settings);
            dbContext.SaveChanges();
        }

        public void UpdateFolder(string folder)
        {
            var dbContext = new GSyncContext();
            var settings = dbContext.Settings.FirstOrDefault();

           if(settings != null)
            {
                settings.UpdateFolder(folder);
                dbContext.Settings.Update(settings);
                dbContext.SaveChanges();
            }
        }

        public SettingsEntity? GetSettings()
        {
            var dbContext = new GSyncContext();
            return dbContext.Settings.FirstOrDefault();
        }

        public void SaveSettings(SettingsEntity settings)
        {
            var dbContext = new GSyncContext();
            dbContext.Settings.Update(settings);
            dbContext.SaveChanges();
        }
    }
}