using System.Linq;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QFighterPolice.Functions
{
    public static class Extensions
    {
        public static (string Prefix, int argPos) GetPrefix(this IUserMessage message)
        {
            int argPos = 0;
            JObject config = ConfigManager.GetConfig();

            string[] prefixes = JsonConvert.DeserializeObject<string[]>(config["prefixes"].ToString());
            return (prefixes.FirstOrDefault(x => message.HasStringPrefix(x, ref argPos)), argPos);
        }
    }
}
