using AppStartTest;
using G.Sync.Common;
using G.Sync.DataContracts;
using G.Sync.Entities.Interfaces;
using G.Sync.External.IO;
using G.Sync.Google.Api;
using G.Sync.IoC;
using G.Sync.Repository;
using G.Sync.Service;
using G.Sync.Service.MessageFactory;
using G.Sync.Service.MessageFactory.Strategy.Enumerators;
using G.Sync.Utils;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace WebApp
{
    public static class WebSocketCore
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            BusinessComponent.InitializeModules(
                new AppStartNinjectModule(),
                new RepositoryNinjectModule(),
                new GoogleApiNinjectModule(),
                new ExternalIoNinjectModule(),
                new ServiceNinjectModule(),
                new UtilsNinjectModule(),
                new CommonNinjectModule()
            );

            var stater = BusinessComponent.CreateInstance<IStarter>();

            builder.WebHost.ConfigureKestrel(options =>
            {
                var isLocalHost = stater.Settings.IpAdress.Equals("localhost", StringComparison.OrdinalIgnoreCase);

                var port = stater.Settings.Port;

                if (isLocalHost)
                {
                    options.ListenLocalhost(port);
                }
                else
                {
                    var ipAddress = IPAddress.Parse(stater.Settings.IpAdress);
                    options.Listen(ipAddress, port);
                }

                options.Listen(IPAddress.Loopback, 5001, listenOptions => listenOptions.UseHttps());
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
                if (!context.WebSockets.IsWebSocketRequest)
                    return;

                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                var notifyHandler = BusinessComponent.CreateInstance<ITaskNotifier>();

                await notifyHandler.AddClientAsync(webSocket, ip);

                var buffer = new byte[4 * 1024];

                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                    var rawText = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    var dto = new MessageDto(rawText);
                    var factory = BusinessComponent.CreateInstance<IMessageHandlerFactory>();

                    var handler = factory.CreateHandler((HandlerType)dto.HandlerType);

                    await handler.HandleAsync(dto.TaskId);

                    if (result.CloseStatus.HasValue)
                        await webSocket.CloseAsync(result.CloseStatus.Value,
                                                   result.CloseStatusDescription,
                                                   CancellationToken.None);
                }
            });

            app.UseAuthorization();
            app.MapControllers();

            try
            {
                var staterTask = stater.StartAsync();

                await Task.WhenAll(app.RunAsync(), staterTask);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.ToString());
                throw;
            }
        }
    }
}