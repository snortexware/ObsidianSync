using G.Sync.Entities.Interfaces;

namespace G.Sync.Common
{
    public interface IGSyncAppContext
    {
        ISettingsEntity GetAppSettings();
    }
}