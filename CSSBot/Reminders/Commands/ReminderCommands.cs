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
        /// <summary>
        /// Adds a reminder
        /// </summary>
        /// <returns></returns>
        [Command("Add"), Alias("Create"), RequireContext(ContextType.Guild)]
        public async Task AddReminder([Name("Time")]DateTime reminderTime, [Name("Reminder")]string ReminderText)
        {
            //todo implement ReminderTimeOption
            //todo implement ReminderType
            //_reminderService.AddReminder(Context.Guild.Id, Context.Channel.Id, Context.User.Id,
            //    ReminderText, reminderTime);

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
        public async Task ListUserReminders()
        {

        }

    }
}
