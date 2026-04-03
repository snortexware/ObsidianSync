using G.Sync.Entities.Interfaces;
using G.Sync.Service.MessageFactory.Strategy.Enumerators;
using Ninject;

namespace G.Sync.Service.MessageFactory.Strategy
{
    public class VaultsHandler : IHandler
    {
        [Inject] public IVaultsRepository VaultsRepository { get; set; }
        [Inject] public ITaskNotifier TaskNotifier { get; set; }

        public HandlerType Type => HandlerType.Vaults;

        public async Task HandleAsync(object data)
        {
            var vaults = VaultsRepository.GetVaults().ToList();

            var taskId = data is long id ? id : 0;

            await TaskNotifier.SendVaultsAsync(vaults, taskId);
        }
    }
}