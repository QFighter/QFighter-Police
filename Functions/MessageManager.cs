using System;
using System.Threading.Tasks;
using Discord;

namespace QFighterPolice.Functions
{
    public static class MessageManager
    {
        public static async Task SendStatusUpdateMessageAsync(ITextChannel channel, bool isServerOnline)
        {
            var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var header = "__**Server status update**__ " + (isServerOnline ? "<:miublush2:845314172480782406>" : "<:miuscream3:845316873662496838>");
            var body = $"**Timestamp:** <t:{unixTimestamp}>\n**Status:** {(isServerOnline ? "Online 🟢" : "Offline 🔴")}";

            await channel.SendMessageAsync($"{header}\n\n{body}");
        }
    }
}
