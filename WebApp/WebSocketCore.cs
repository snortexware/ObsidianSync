using AppStartTest;
using G.Sync.Repository;
using G.Sync.DataContracts;
using G.Sync.Service.MessageFactory;
using G.Sync.Service.MessageFactory.Strategy.Enumerators;
using System.Net.WebSockets;
using System.Text;
using Utils.NotifyHandler;
using Microsoft.EntityFrameworkCore;

public static class WebSocketCore
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(5000);
            options.ListenLocalhost(5001, listenOptions =>
            {
                listenOptions.UseHttps();
            });
        });

        builder.Services.AddDbContext<GSyncContext>();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Todo: mover para camada de application.
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<GSyncContext>();
            db.Database.Migrate();
        }

        var stater = new Stater();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // -----------------------------------------------------
        // WEBSOCKETS
        // -----------------------------------------------------
        app.UseWebSockets();

        app.Map("/gsync", async context =>
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return;

            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            NotifyHandler.Instance.AddClient(webSocket, ip);

            var buffer = new byte[4 * 1024];

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                var rawText = Encoding.UTF8.GetString(buffer, 0, result.Count);

                var dto = new MessageDto(rawText);
                var factory = new MessageHandlerFactory();
                var handler = factory.CreateHandler((HandlerType)dto.HandlerType);

                handler.HandleMessage(dto.TaskId);

                Console.WriteLine("=============== Message Received ================");
                Console.WriteLine(dto);
                Console.WriteLine("=================================================");

                if (result.CloseStatus.HasValue)
                {
                    await webSocket.CloseAsync(result.CloseStatus.Value,
                                               result.CloseStatusDescription,
                                               CancellationToken.None);
                }
            }
        });

        // -----------------------------------------------------
        // ROTAS E MIDDLEWARES
        // -----------------------------------------------------
        app.UseAuthorization();
        app.MapControllers();

        // -----------------------------------------------------
        // INICIAR SERVIDOR + STATER
        // -----------------------------------------------------
        var runTask = app.RunAsync();

        stater.Start();

        await runTask;
    }
}
