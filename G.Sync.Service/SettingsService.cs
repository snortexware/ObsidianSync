
using G.Sync.Entities;

namespace G.Sync.Service
{
    public interface ISettingsRepository
    {
        public SettingsEntity? GetSettings();
        public void CreateDefaultSettings();
        public void UpdateFolder(string folder);

        SettingsEntity Get();
        void Save(SettingsEntity settings);
    }

    public class SettingsService(ISettingsRepository repo)
    {
        private readonly ISettingsRepository _repo = repo;

        public void UpdateFolder(string folder)
        {
            var settings = _repo.Get();

            settings.UpdateFolder(folder); 

            _repo.Save(settings); 
        }
    }
}
