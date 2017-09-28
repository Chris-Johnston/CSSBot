using CSSBot.Reminders;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot
{
    [Group("Reminder"), Alias("R")]
    public class ReminderCommands : ModuleBase
    {
        private readonly ReminderService _reminderService;

        public ReminderCommands(ReminderService reminderService)
        {
            _reminderService = reminderService;
        }

        /// <summary>
        /// Adds a reminder
        /// </summary>
        /// <returns></returns>
        [Command("Add"), Alias("Create"), RequireContext(ContextType.Guild)]
        public async Task AddReminder([Name("Time")]DateTime reminderTime, [Name("Reminder")]string ReminderText)
        {
            //todo implement ReminderTimeOption
            //todo implement ReminderType
            _reminderService.AddReminder(Context.Guild.Id, Context.Channel.Id, Context.User.Id,
                ReminderText, reminderTime);

            string replyText = string.Format("Ok {0}! I've created a reminder for `{1:g}`.", Context.User.Mention, reminderTime);
            await ReplyAsync(replyText);
        }

        /// <summary>
        /// Dismisses a reminder
        /// </summary>
        /// <returns></returns>
        public async Task DismissReminder()
        {
            // only allow the author of the reminder, or a user with ManageMessages or Administrator
            // to remove messages
        }

        /// <summary>
        /// Lists all of the guild reminders
        /// </summary>
        /// <returns></returns>
        public async Task ListGuildReminders()
        {

        }

        /// <summary>
        /// Lists all of the channel reminders
        /// </summary>
        /// <returns></returns>
        public async Task ListChannelReminders()
        {

        }

        /// <summary>
        /// Lists all of the reminders authored by the given user (or Context.User)
        /// </summary>
        /// <returns></returns>
        [Command("UserReminders")]
        [Alias("MyReminders", "ListUser")]
        [RequireContext(ContextType.Guild)]
        [Summary("Gets all the reminders authored by the specified user, or yourself if unspecified.")]
        public async Task ListUserReminders(IGuildUser user = null)
        {
            if (user == null)
                user = Context.User as IGuildUser;

            // match all by AuthorId in this guild
            var userReminders = _reminderService.ActiveReminders.FindAll(x => x.AuthorId == user.Id && x.GuildId == user.GuildId);

            if (userReminders.Count == 0)
            {
                await ReplyAsync(string.Format("There are no active reminders authored by {0}.", user.Mention));
            }
            else
            {
                var builder = new EmbedBuilder();
                builder.WithAuthor(Context.Client.CurrentUser);
                builder.WithColor(new Color(255, 204, 77));

                builder.WithTitle(string.Format("Reminders for {0}:", user.Nickname ?? user.Username));

                userReminders.ForEach(x =>
               {
                   builder.AddField(x.ReminderTime.ToString("s"), x.ReminderText, true);
               });

                await ReplyAsync("", false, builder.Build());
            }
        }

    }
}
