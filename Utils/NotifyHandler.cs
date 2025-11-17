using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;

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

        public void AddClient(WebSocket ws, string ip)
        {
            var isLocal = IPAddress.IsLoopback(IPAddress.Parse(ip));

            if(!isLocal)
                throw new UnauthorizedAccessException("Only local connections are allowed.");

            Console.WriteLine($"Client connected with ip: {ip}");
            _clients.Add(ws);
        }

        public async Task NotifyAsync(TaskEntity task) => await NotifyAsyncBase(task);
        public async Task SendVaultsAsync(IEnumerable<VaultsEntity> vaults) => await NotifyAsyncBase(vaults);

        public async Task NotifyAsyncBase(object obj)
        {
            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var buffer = Encoding.UTF8.GetBytes(json);
            var segment = new ArraySegment<byte>(buffer);

            lock (_clients)
            {
                foreach (var client in _clients.ToList())
                {
                    if (client.State == WebSocketState.Open)
                    {
                        client.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                        Console.WriteLine($"=========================== Message sent ===========================");
                        Console.WriteLine(json);
                        Console.WriteLine($"====================================================================");
                    }
                    else
                        _clients.Remove(client);
                }
            }
        }


        public void RemoveClient(WebSocket ws)
        {
            Console.WriteLine("Client disconnected");
            _clients.Remove(ws);
        }

        public void HandleMessage(WebSocket ws, byte[] buffer, int count)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, count);

            Console.WriteLine($"Received message from client: {message}");
        }
    }
}
