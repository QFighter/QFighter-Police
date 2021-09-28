using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;

namespace QFighterPolice.Functions
{
    public static class StatusManager
    {
        public static async Task SetBotStatusAsync(DiscordSocketClient client)
        {
            JObject config = ConfigManager.GetConfig();

            string currently = config["currently"]?.Value<string>().ToLower();
            string statusText = config["playing_status"]?.Value<string>();
            string onlineStatus = config["status"]?.Value<string>().ToLower();

            if (!string.IsNullOrEmpty(onlineStatus))
            {
                UserStatus userStatus = onlineStatus switch
                {
                    "dnd" => UserStatus.DoNotDisturb,
                    "idle" => UserStatus.Idle,
                    "offline" => UserStatus.Invisible,
                    _ => UserStatus.Online
                };

                await client.SetStatusAsync(userStatus);
                Logger.LogMessage($"Online status set to {userStatus}");
            }

            if (!string.IsNullOrEmpty(currently) && !string.IsNullOrEmpty(statusText))
            {
                ActivityType activity = currently switch
                {
                    "listening" => ActivityType.Listening,
                    "watching" => ActivityType.Watching,
                    "streaming" => ActivityType.Streaming,
                    _ => ActivityType.Playing
                };

                await client.SetGameAsync(statusText, type: activity);
                Logger.LogMessage($"Playing status set to {activity}: {statusText}");
            }
        }
    }
}
