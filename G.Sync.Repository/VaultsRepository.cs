using G.Sync.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Repository
{
    public class VaultsRepository
    {
        public void CreateVault(VaultsEntity vault)
        {
            using var dbContext = new GSyncContext();

            var exists = dbContext.Vaults.FirstOrDefault(x => x.Name.Equals(vault.Name));

            if (exists is not null) return;

            dbContext.Vaults.Add(vault);
            dbContext.SaveChanges();
        }

        public IEnumerable<VaultsEntity?> GetVaults()
        {
            var dbContext = new GSyncContext();
            return dbContext.Vaults.AsNoTracking();
        }

        public void SaveSettings(VaultsEntity vault)
        {
            using var dbContext = new GSyncContext();
            dbContext.Vaults.Update(vault);
            dbContext.SaveChanges();
        }

    }
}
