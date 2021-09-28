using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace QFighterPolice.Functions
{
    public static class ReactionConfirm
    {
        private static readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(30);

        public static async Task<bool?> ReactionConfirmAsync(DiscordSocketClient client, IUser author, IUserMessage message, TimeSpan? timeout = null)
        {
            var checkmarkEmoji = new Emoji("✅");
            var crossmarkEmoji = new Emoji("❎");

            var eventTrigger = new TaskCompletionSource<bool>();

            await message.AddReactionsAsync(new Emoji[] { checkmarkEmoji, crossmarkEmoji });

            async Task HandlerAsync(Cacheable<IUserMessage, ulong> cacheMsg, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
            {
                var msg = await cacheMsg.GetOrDownloadAsync();
                if (reaction.User.Value.Id == author.Id)
                    if (reaction.Emote.Name == checkmarkEmoji.Name)
                        eventTrigger.SetResult(true);
                    else if (reaction.Emote.Name == crossmarkEmoji.Name)
                        eventTrigger.SetResult(false);
            }

            client.ReactionAdded += HandlerAsync;

            timeout = timeout == null ? _defaultTimeout : timeout;

            var trigger = eventTrigger.Task;
            var delay = Task.Delay(timeout.Value);
            var task = await Task.WhenAny(trigger, delay);

            client.ReactionAdded -= HandlerAsync;

            if (task == trigger)
                return await trigger;
            else
                return null;
        }
    }
}
