using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using Discord.WebSocket;

namespace QFighterPolice
{
    public class Ping
    {
        private bool _previousOnlineStatus;

        public Ping()
        {
            JObject config = ConfigManager.GetConfig();

            _previousOnlineStatus = BeginConnect(config);
        }

        public async Task PingServerAsync(DiscordSocketClient client)
        {
            JObject config = ConfigManager.GetConfig();

            ulong guildId = (ulong)config["guild_id"];
            ulong channelId = (ulong)config["server_status_channel"];
            var guild = client.GetGuild(guildId);
            var channel = guild.GetTextChannel(channelId);

            bool success = BeginConnect(config);

            if (success && !_previousOnlineStatus)
                await channel.SendMessageAsync("__**Server status**__ <:miublush2:845314172480782406>\nOnline 🟢");
            else if (!success && _previousOnlineStatus)
            {
                await channel.SendMessageAsync("__**Server status**__ <:miuscream3:845316873662496838>\nOffline 🔴");

                var modChatMessage = (string)config["mod_chat_message"];

                if (!string.IsNullOrEmpty(modChatMessage) && guild.GetTextChannel((ulong)config["mod_chat"]) is SocketTextChannel modChat)
                    await modChat.SendMessageAsync(modChatMessage);
            }

            _previousOnlineStatus = success;
        }

        private static bool BeginConnect(JObject config)
        {
            using var tcpClient = new TcpClient();
            var result = tcpClient.BeginConnect((string)config["server_ip"], (int)config["server_port"], null, null);
            return result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));
        }
    }
}
