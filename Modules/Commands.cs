using System.Threading.Tasks;
using Discord.Commands;
using Newtonsoft.Json.Linq;

namespace QFighterPolice.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task Help()
        {
            JObject config = ConfigManager.GetConfig();
            await ReplyAsync($"Type `.report` in my DMs to report a player! Oh, and I also keep track of the server status in <#{config["server_status_channel"]}> :)");
        }

        [Command("ping")]
        public async Task Ping()
            => await ReplyAsync($":ping_pong: Pong! ({Context.Client.Latency} ms)");
    }
}
