using AppStartTest;
using G.Sync.DataContracts;
using G.Sync.Service.MessageFactory;
using G.Sync.Service.MessageFactory.Strategy.Enumerators;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Utils.NotifyHandler;

public static class Program
{
    public static void Main(string[] args)
    {
        var stater = new Stater();

        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(5000); 
            options.ListenLocalhost(5001, listenOptions =>
            {
                listenOptions.UseHttps(); 
            });
        });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseWebSockets();

        app.Map("/gsync", async context =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                NotifyHandler.Instance.AddClient(webSocket, ip);

                var buffer = new byte[1024 * 4];

                while (webSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    var factory = new MessageHandlerFactory();
                    var rawText = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    var dto = new MessageDto(rawText);

                    var factoryAction = factory.CreateHandler((HandlerType)dto.HandlerType);
                    factoryAction.HandleMessage(dto.TaskId);

                    Console.WriteLine($"=========================== Message received ===========================");
                   
                    Console.WriteLine(dto);

                    Console.WriteLine($"====================================================================");
                    if (result.CloseStatus.HasValue)
                    {
                        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                    }
                }

            }
        });

        app.UseAuthorization();

        app.MapControllers();
        app.RunAsync();

        stater.Start();
    }
}

