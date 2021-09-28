using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Interactivity;
using Newtonsoft.Json.Linq;
using QFighterPolice.Functions;
using QFighterPolice.Models;

namespace QFighterPolice.Modules
{
    public class Report
    {
        private readonly DiscordSocketClient _client;
        private readonly SocketUser _user;
        private readonly SocketDMChannel _channel;
        private readonly InteractivityService _interactivity;
        private readonly string _prefix;

        private readonly EmbedBuilder _embed = new();

        private IUserMessage _botMsg;

        public Report(DiscordSocketClient client, SocketUser user, SocketDMChannel channel, InteractivityService interactivity, string prefix)
        {
            _client = client;
            _user = user;
            _channel = channel;
            _interactivity = interactivity;
            _prefix = prefix;
        }

        public async Task StartReportAsync()
        {
            _embed.WithColor(255, 177, 74);
            _embed.WithFooter("QFighter Police by VAC Efron");

            var reportStage = 1;
            List<Question> questions = ConfigManager.GetQuestions();
            List<AnsweredQuestion> answeredQuestions = new();

            while (reportStage <= questions.Count + 1)
            {
                Question question = null;

                var titleSuffix = $" `{reportStage}/{questions.Count + 1}`";

                if (reportStage != questions.Count + 1)
                {
                    question = questions[reportStage - 1];

                    _embed.WithTitle(question.Title + titleSuffix);
                    _embed.WithDescription(question.Description + "\n\nType .cancel to stop" + (reportStage != 1 ? $"\nType {_prefix}back to go back to the previous question." : ""));
                }
                else
                {
                    _embed.WithTitle("Are you sure you want to submit this report?" + titleSuffix);
                    _embed.WithDescription("Use the reactions below to answer. You have 5 minutes to respond.");

                    await SendOrModifyEmbedAsync();
                    await ConfirmReportAsync(_botMsg, answeredQuestions);
                    return;
                }

                await SendOrModifyEmbedAsync();

                var nextMessage = await _interactivity.NextMessageAsync(x => x.Author.Id == _user.Id);

                if (!nextMessage.IsSuccess)
                    return;

                string msgContent = nextMessage.Value.Content;

                if (msgContent == $"{_prefix}cancel")
                {
                    _embed.WithTitle("Cancelled");
                    _embed.WithDescription("Your report has been cancelled.");

                    await SendOrModifyEmbedAsync();
                    return;
                }
                else if (msgContent == $"{_prefix}back" && reportStage > 1)
                {
                    answeredQuestions.RemoveAt(answeredQuestions.Count - 1);
                    reportStage--;
                }
                else
                {
                    answeredQuestions.Add(new AnsweredQuestion(question, msgContent));
                    reportStage++;
                }                
            }            
        }

        private async Task SendOrModifyEmbedAsync()
        {
            if (_botMsg == null)
                _botMsg = await _channel.SendMessageAsync("**Report User**", embed: _embed.Build());
            else
                await _botMsg.ModifyAsync(x => x.Embed = _embed.Build());
        }

        private async Task ConfirmReportAsync(IUserMessage botMsg, List<AnsweredQuestion> answeredQuestions)
        {
            var reactionConfirm = await ReactionConfirm.ReactionConfirmAsync(_client, _user, botMsg, TimeSpan.FromMinutes(5));

            if (reactionConfirm is not bool confirm)
                return;
            else if (confirm)
            {
                var msg = $"**__New report by__** {_user.Mention}\n\n";

                foreach (var answeredQuestion in answeredQuestions)
                    msg += $"**{answeredQuestion.Question.Tag}:** {answeredQuestion.Answer}\n";

                JObject config = ConfigManager.GetConfig();

                var reportMsg =  await _client.GetGuild((ulong)config["guild_id"]).GetTextChannel((ulong)config["report_channel"]).SendMessageAsync(msg);
                await reportMsg.AddReactionAsync(new Emoji("✅"));

                _embed.WithTitle("Success");
                _embed.WithDescription("Your report has been submitted successfully.");
            }
            else
            {
                _embed.WithTitle("Cancelled");
                _embed.WithDescription("Your report has been cancelled.");
            }

            await botMsg.ModifyAsync(x => x.Embed = _embed.Build());
        }
    }
}
