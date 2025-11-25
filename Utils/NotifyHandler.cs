using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using G.Sync.Utils;
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

        public async void AddClient(WebSocket ws, string ip)
        {
            try
            {
                var isLocal = IPAddress.IsLoopback(IPAddress.Parse(ip));

                if (!isLocal)
                    throw new UnauthorizedAccessException("Only local connections are allowed.");

                _clients.Add(ws);
                await SendConnectionStatusAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding client: {ex.Message}");
                await SendConnectionStatusAsync(false);
                await ws.CloseAsync(WebSocketCloseStatus.PolicyViolation, "Unauthorized", CancellationToken.None);
            }
        }

        public async Task NotifyAsync(TaskEntity task) => await NotifyAsyncBase(task);
        public async Task SendVaultsAsync(IEnumerable<VaultsEntity> vaults, long taskId) => await NotifyAsyncBase(vaults, true, taskId);
        public async Task NotifyAsyncBase(object obj, bool hanlder = false, long taskId = 0)
        {
            if (hanlder)
            {
                obj = new RespondeDto
                {
                    Ok = true,
                    Data = obj,
                    TaskId = taskId
                };
            }

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

        public async Task SendConnectionStatusAsync(bool ok)
        {
            var message = new
            {
                type = "connection",
                ok = ok
            };

            await NotifyAsyncBase(message);
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
