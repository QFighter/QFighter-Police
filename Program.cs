using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Interactivity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using QFighterPolice.Functions;

namespace QFighterPolice
{
    class Program
    {
        static void Main()
            => new Program().MainAsync().GetAwaiter().GetResult();

        private async Task MainAsync()
        {
            using var services = ConfigureServices();

            Logger.LogMessage("QFighter Police clocking in...");

            var client = services.GetRequiredService<DiscordSocketClient>();

            client.Log += Log;
            services.GetRequiredService<CommandService>().Log += Log;

            JObject config = ConfigManager.GetConfig();
            string token = config["token"].Value<string>();

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await services.GetRequiredService<EventHandlingService>().InitializeAsync();

            await Task.Delay(-1);
        }

        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    MessageCacheSize = 500,
                    LogLevel = LogSeverity.Info,
                    GatewayIntents = GatewayIntents.All
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    LogLevel = LogSeverity.Info,
                    DefaultRunMode = RunMode.Async,
                    CaseSensitiveCommands = false,
                    IgnoreExtraArgs = true
                }))
                .AddSingleton<EventHandlingService>()
                .AddSingleton(new InteractivityConfig { DefaultTimeout = TimeSpan.FromHours(1) })
                .AddSingleton<InteractivityService>()
                .BuildServiceProvider();
        }

        private Task Log(LogMessage log)
        {
            Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] {log.Message}");
            return Task.CompletedTask;
        }
    }
}
