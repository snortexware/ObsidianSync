
using G.Sync.Entities;

namespace G.Sync.Service
{
    public interface ISettingsRepository
    {
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
