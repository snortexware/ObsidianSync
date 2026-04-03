using G.Sync.Entities;

namespace G.Sync.Entities.Interfaces
{
    public interface IFileWatcher
    {
        void StartWatching(IEnumerable<VaultsEntity> vaults, SettingsEntity settings);
        void StopWatching();
    }
}