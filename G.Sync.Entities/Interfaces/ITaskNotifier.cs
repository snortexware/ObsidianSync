using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace G.Sync.Entities.Interfaces
{
    public interface ITaskNotifier
    {
        Task NotifyAsync(TaskEntity task);
        Task SendVaultsAsync(IEnumerable<VaultsEntity> vaults, long taskId);
        Task AddClientAsync(WebSocket ws, string ip);

    }
}
