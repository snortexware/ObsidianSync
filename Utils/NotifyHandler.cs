using System.Net.WebSockets;
using System.Text.Json;
using SystemDataContracts;

namespace Utils.NotifyHandler
{
        public class NotifyHandler
        {
            private static NotifyHandler instance;
            private NotifyHandler() { }

            public static NotifyHandler notifyHandler
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new NotifyHandler();
                    }
                    return instance;
                }
            }

            private readonly List<WebSocket> _clients = new();
            public void AddClient(WebSocket webSocket)
            {

                Console.WriteLine("New Client abord");

                _clients.Add(webSocket);
            }

            public async Task NotifyClientsAsync(TaskDataObject dto)
            {
                var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var buffer = System.Text.Encoding.UTF8.GetBytes(json);
                var segment = new ArraySegment<byte>(buffer);

                lock (_clients)
                {
                    foreach (var client in _clients.ToList())
                    {
                        if (client.State == WebSocketState.Open)
                        {
                            client.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                        }
                        else
                        {
                            _clients.Remove(client);
                        }
                    }
                }
            }
        }
}
