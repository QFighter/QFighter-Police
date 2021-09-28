using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using Discord.Commands;
using Discord.WebSocket;
using Interactivity;
using Microsoft.Extensions.DependencyInjection;
using QFighterPolice.Functions;
using QFighterPolice.Modules;

namespace QFighterPolice
{
    public class EventHandlingService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;
        private readonly InteractivityService _interactivity;
        private readonly IServiceProvider _services;

        private readonly List<ulong> _reportingUsers = new();

        public EventHandlingService(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _interactivity = services.GetRequiredService<InteractivityService>();
            _services = services;

            _client.Ready += ClientReadyAsync;
            _client.MessageReceived += HandleCommandAsync;

            StartTimer();
        }        

        private async Task HandleCommandAsync(SocketMessage rawMessage)
        {
            if (rawMessage.Author.IsBot || rawMessage is not SocketUserMessage message)
                return;

            int argPos = 0;

            string prefix = message.GetPrefix();

            if (prefix != null && message.Content == $"{prefix}report" && !_reportingUsers.Contains(message.Author.Id) && message.Channel is SocketDMChannel channel)
            {
                _ = Task.Run(async () =>
                {
                    _reportingUsers.Add(message.Author.Id);

                    var report = new Report(_client, message.Author, channel, _interactivity, prefix);
                    await report.StartReportAsync();

                    _reportingUsers.Remove(message.Author.Id);
                });
            }
            else if (prefix != null)
            {
                var context = new SocketCommandContext(_client, message);

                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess && result.Error.HasValue && result.Error != CommandError.UnknownCommand)
                    await context.Channel.SendMessageAsync($":x: {result.ErrorReason}");
            }
        }

        private void StartTimer()
        {
            var ping = new Ping();
            var timer = new Timer(TimeSpan.FromSeconds(20).TotalMilliseconds) { AutoReset = true };
            timer.Elapsed += async (s, e) => await ping.PingServerAsync(_client);
            timer.Start();
        }

        private async Task ClientReadyAsync()
            => await StatusManager.SetBotStatusAsync(_client);

        public async Task InitializeAsync()
            => await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }
}
