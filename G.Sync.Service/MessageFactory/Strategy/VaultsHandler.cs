using G.Sync.Repository;
using G.Sync.Service.MessageFactory.Strategy.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.NotifyHandler;

namespace G.Sync.Service.MessageFactory.Strategy
{
    public class VaultsHandler : IHandler
    {
        public async void HandleMessage(long taskId)
        {
            var vaultsRepo = new VaultsRepository();

            var vaults = vaultsRepo.GetVaults().ToList();

            await NotifyHandler.Instance.SendVaultsAsync(vaults, taskId); ;
        }
    }
}
