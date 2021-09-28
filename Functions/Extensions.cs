using System.Linq;
using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QFighterPolice.Functions
{
    public static class Extensions
    {
        public static string GetPrefix(this IUserMessage message)
        {
            JObject config = ConfigManager.GetConfig();

            string[] prefixes = JsonConvert.DeserializeObject<string[]>(config["prefixes"].ToString());
            return prefixes.FirstOrDefault(x => message.Content.StartsWith(x));
        }
    }
}
