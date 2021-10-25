using System;
using System.Threading.Tasks;
using System.Net.Sockets;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using QFighterPolice.Functions;

namespace QFighterPolice.Modules
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

            bool success = TryConnect(config);

            if (success && !_previousOnlineStatus)
                await MessageManager.SendStatusUpdateMessageAsync(channel, success);
            else if (!success && _previousOnlineStatus)
            {
                await MessageManager.SendStatusUpdateMessageAsync(channel, success);

                var modChatMessage = (string)config["mod_chat_message"];

                if (!string.IsNullOrEmpty(modChatMessage) && guild.GetTextChannel((ulong)config["mod_chat"]) is SocketTextChannel modChat)
                    await modChat.SendMessageAsync(modChatMessage);
            }

            _previousOnlineStatus = success;
        }

        private bool TryConnect(JObject config, uint attempts = 5)
        {
            try
            {                
                bool success = false;

                for (int i = 0; i < attempts && !success; i++)
                {
                    using var tcpClient = new TcpClient();

                    var result = tcpClient.BeginConnect((string)config["server_ip"], (int)config["server_port"], null, null);
                    success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));

                    Logger.LogMessage($"Server pinged. Attempt {i + 1} {(success ? "successful" : "failed")}.");
                }

                return success;
            }
            catch (Exception e)
            {
                Logger.LogMessage($"Ping error: {e.Message}");
                return _previousOnlineStatus;
            }
        }
    }
}
