using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;

namespace QFighterPolice
{
    public class Ping
    {
        private bool _previousOnlineStatus;

        public Ping()
        {
            JObject config = ConfigManager.GetConfig();

            _previousOnlineStatus = TryConnect(config);
        }

        public async Task PingServerAsync(DiscordSocketClient client)
        {
            JObject config = ConfigManager.GetConfig();

            var guild = client.GetGuild((ulong)config["guild_id"]);
            var channel = guild.GetTextChannel((ulong)config["server_status_channel"]);
            long unixTimeNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            bool success = TryConnect(config);

            var emote = success ? "<:miublush2:845314172480782406>" : "<:miuscream3:845316873662496838>";
            var body = $"**Timestamp:** <t:{unixTimeNow}>\n**Status:** {(success ? "Online 🟢" : "Offline 🔴")}";

            if (success && !_previousOnlineStatus)
                await channel.SendMessageAsync($"__**Server status update**__ {emote}\n\n{body}");
            else if (!success && _previousOnlineStatus)
            {
                await channel.SendMessageAsync($"__**Server status update**__ {emote}\n\n{body}");

                var modChatMessage = (string)config["mod_chat_message"];

                if (!string.IsNullOrEmpty(modChatMessage) && guild.GetTextChannel((ulong)config["mod_chat"]) is SocketTextChannel modChat)
                    await modChat.SendMessageAsync(modChatMessage);
            }

            _previousOnlineStatus = success;
        }

        private static bool TryConnect(JObject config, uint attempts = 5)
        {
            using var tcpClient = new TcpClient();
            bool success = false;

            for (int i = 0; i < attempts && !success; i++)
            {
                var result = tcpClient.BeginConnect((string)config["server_ip"], (int)config["server_port"], null, null);
                success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));

                Logger.LogMessage($"Server pinged. Attempt {i + 1} {(success ? "successful" : "failed")}.");
            }

            return success;
        }
    }
}
