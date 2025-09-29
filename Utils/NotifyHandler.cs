using G.Sync.Entities;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using System.Threading;
using G.Sync.Entities.Interfaces;

namespace Utils.NotifyHandler
{
    public class NotifyHandler : ITaskNotifier
    {
        private static NotifyHandler instance;
        private NotifyHandler() { }

        public static NotifyHandler Instance
        {
            get
            {
                instance ??= new NotifyHandler();
                return instance;
            }
        }

        private readonly List<WebSocket> _clients = new();

        public void AddClient(WebSocket ws) => _clients.Add(ws);

        public async Task NotifyAsync(TaskEntity task)
        {
            var json = JsonSerializer.Serialize(task, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var buffer = Encoding.UTF8.GetBytes(json);
            var segment = new ArraySegment<byte>(buffer);

            lock (_clients)
            {
                foreach (var client in _clients.ToList())
                {
                    if (client.State == WebSocketState.Open)
                        client.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                    else
                        _clients.Remove(client);
                }
            }
        }
    }
}
