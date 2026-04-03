using G.Sync.Entities;
using G.Sync.Entities.Interfaces;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace G.Sync.Utils
{
    public class NotifyHandler : ITaskNotifier
    {
        private readonly List<WebSocket> _clients = new();
        private readonly object _lock = new();

        public async Task AddClientAsync(WebSocket ws, string ip)
        {
            try
            {
                var isLocal = IPAddress.IsLoopback(IPAddress.Parse(ip));

                if (!isLocal)
                    throw new UnauthorizedAccessException("Only local connections are allowed.");

                lock (_lock)
                    _clients.Add(ws);

                await SendConnectionStatusAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding client: {ex.Message}");

                await SendConnectionStatusAsync(false);

                await ws.CloseAsync(
                    WebSocketCloseStatus.PolicyViolation,
                    "Unauthorized",
                    CancellationToken.None);
            }
        }

        public Task NotifyAsync(TaskEntity task) =>
            NotifyAsyncBase(task);

        public Task SendVaultsAsync(IEnumerable<VaultsEntity> vaults, long taskId) =>
            NotifyAsyncBase(new
            {
                ok = true,
                data = vaults,
                taskId
            });

        private async Task NotifyAsyncBase(object obj)
        {
            var json = JsonSerializer.Serialize(obj,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            var buffer = Encoding.UTF8.GetBytes(json);
            var segment = new ArraySegment<byte>(buffer);

            List<WebSocket> clientsCopy;

            lock (_lock)
                clientsCopy = _clients.ToList();

            foreach (var client in clientsCopy)
            {
                try
                {
                    if (client.State == WebSocketState.Open)
                    {
                        await client.SendAsync(
                            segment,
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);

                        Console.WriteLine("==== SENT ====");
                        Console.WriteLine(json);
                    }
                    else
                    {
                        RemoveClient(client);
                    }
                }
                catch
                {
                    RemoveClient(client);
                }
            }
        }

        public async Task SendConnectionStatusAsync(bool ok)
        {
            await NotifyAsyncBase(new
            {
                type = "connection",
                ok
            });
        }

        public void RemoveClient(WebSocket ws)
        {
            lock (_lock)
                _clients.Remove(ws);

            Console.WriteLine("Client disconnected");
        }

        public void HandleMessage(WebSocket ws, byte[] buffer, int count)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, count);
            Console.WriteLine($"Received: {message}");
        }
    }
}