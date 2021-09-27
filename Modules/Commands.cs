using System.Threading.Tasks;
using Discord.Commands;

namespace QFighterPolice.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public async Task Help()
            => await ReplyAsync("Type `.report` in my DMs to report a player!");

        [Command("ping")]
        public async Task Ping()
            => await ReplyAsync($"Pong! ({Context.Client.Latency})");
    }
}
