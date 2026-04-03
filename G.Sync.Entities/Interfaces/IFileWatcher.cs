using G.Sync.Entities;

namespace G.Sync.Entities.Interfaces
{
    public interface IFileWatcher
    {
        void StartWatching(IEnumerable<VaultsEntity> vaults, ISettingsEntity settings);
        void StopWatching();
    }
}