using G.Sync.Entities;

namespace G.Sync.Entities.Interfaces
{
    public interface IVaultsRepository
    {
        void CreateVault(VaultsEntity vault);
        VaultsEntity? GetById(long id);
        IEnumerable<VaultsEntity?> GetVaults();
        void SaveSettings(VaultsEntity vault);
    }
}