﻿using System;
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

            ulong guildId = (ulong)config["guild_id"];
            ulong channelId = (ulong)config["server_status_channel"];
            var guild = client.GetGuild(guildId);
            var channel = guild.GetTextChannel(channelId);

            bool success = TryConnect(config);

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
