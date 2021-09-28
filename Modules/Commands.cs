using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using QFighterPolice.Functions;

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

        [RequireOwner]
        [Command("updatetest")]
        public async Task UpdateTest(bool serverOnline)
            => await MessageManager.SendStatusUpdateMessageAsync(Context.Channel as ITextChannel, serverOnline);

        [Group("questions")]
        public class Questions : ModuleBase<SocketCommandContext>
        {
            [Command("")]
            public async Task SendQuestions()
            {
                if (!await CheckPermissions())
                    return;

                await Context.Channel.SendFileAsync("Assets/questions.json",
                    $"Here you go. You can update the questions by editing this JSON file and sending it using `{Context.Message.GetPrefix().Prefix}questions upload`");
            }

            [Command("upload"), Priority(1)]
            public async Task UploadQuestions()
            {
                if (!await CheckPermissions())
                    return;

                if (Context.Message.Attachments.FirstOrDefault() is not Attachment attachment || !attachment.Filename.EndsWith(".json"))
                {
                    await ReplyAsync(":x: You did not pass a JSON file along with the command.");
                    return;
                }

                using var httpClient = new HttpClient();
                string updatedJson = await httpClient.GetStringAsync(attachment.Url);

                await File.WriteAllTextAsync("Assets/questions.json", updatedJson);
                await ReplyAsync("<:miuread:845314833296392212> The questions have been updated :)");
            }

            private async Task<bool> CheckPermissions()
            {
                if (!(Context.User as SocketGuildUser).GuildPermissions.ManageGuild)
                {
                    await ReplyAsync(":x: You need **Manage Server** permissions.");
                    return false;
                }

                return true;
            }
        }
    }
}
