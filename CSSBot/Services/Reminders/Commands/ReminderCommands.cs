using CSSBot.Reminders;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CSSBot.Reminders.Models;

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
        [Command("Add", RunMode = RunMode.Async), Alias("Create", "+"), RequireContext(ContextType.Guild)]
        public async Task AddReminder([Name("Time")]DateTime reminderTime, [Name("Reminder"), Remainder()]string ReminderText)
        {
            //todo implement ReminderTimeOption
            //todo implement ReminderType
            var added = _reminderService.AddReminder(Context.Guild.Id, Context.Channel.Id, Context.User.Id,
                ReminderText, reminderTime);

            string reply = string.Format("Ok {0}! I've created a reminder for `{1:g}` with the ID `{2}`.",
                Context.User.Mention, added.ID, added.ReminderTime);
            await ReplyAsync(reply);
        }

        // add channel reminder
        [Command("AddChannel", RunMode = RunMode.Async)]
        [Alias("CreateChannel", "+Channel")]
        [RequireContext(ContextType.Guild)]
        public async Task AddChannelReminder([Name("Time")]DateTime reminderTime, [Name("Reminder"), Remainder()]string ReminderText)
        {
            var added = _reminderService.AddReminder(Context.Guild.Id, Context.Channel.Id, Context.User.Id,
                ReminderText, reminderTime, ReminderType.Channel);

            string reply = string.Format("Ok {0}! I've created a reminder for `{1:g}` with the ID `{2}`.",
                Context.User.Mention, added.ID, added.ReminderTime);
            await ReplyAsync(reply);
        }

        // add guild reminder
        [Command("AddServer", RunMode = RunMode.Async)]
        [Alias("CreateServer", "+Server", "AddGuild", "CreateGuild", "+Guild")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task AddGuildReminder([Name("Time")]DateTime reminderTime, [Name("Reminder"), Remainder()]string ReminderText)
        {
            var added = _reminderService.AddReminder(Context.Guild.Id, Context.Channel.Id, Context.User.Id,
                ReminderText, reminderTime, ReminderType.Guild);

            string reply = string.Format("Ok {0}! I've created a reminder for `{1:g}` with the ID `{2}`.",
                Context.User.Mention, added.ID, added.ReminderTime);
            await ReplyAsync(reply);
        }
        
        [Command("ListTypeOptions")]
        public async Task ListTypes()
        {
            string ret = "The following type options are available: ";
            foreach (var value in Enum.GetValues(typeof(ReminderType)))
                ret += value.ToString() + " ";
            await ReplyAsync(ret);
        }

        [Command("AddReminderTimespan", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task AddReminderTimespan(int id, TimeSpan ts)
        {
            var reminder = _reminderService.GetReminder(Context.Guild.Id, id);
            if (reminder != null)
            {
                reminder.AddTimeSpan(ts);
                _reminderService.UpdateReminder(reminder);
                //await ReplyAsync("Ok, I added the time " + ts.ToString() + ".");
                await GetReminderById(id);
            }
            else
            {
                await ReplyAsync("Couldn't find a reminder by that ID.");
            }
        }

        [Command("RemoveReminderTimespan", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        public async Task RemoveReminderTimespan(int id, TimeSpan ts)
        {
            var reminder = _reminderService.GetReminder(Context.Guild.Id, id);
            if (reminder != null)
            {
                reminder.RemoveTimeSpan(ts);
                _reminderService.UpdateReminder(reminder);
                //await ReplyAsync("Ok!");
                await GetReminderById(id);
            }
            else
            {
                await ReplyAsync("Couldn't find a reminder by that ID.");
            }
        }

        [Command("UpdateText", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task UpdateReminderText([Name("ReminderID")]int id, [Name("Text"), Remainder()]string text)
        {
            _reminderService.UpdateReminder(Context.Guild.Id, id, text: text);
            await ReplyAsync("Ok!");
        }

        [Command("UpdateTime", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task UpdateReminderTime([Name("ReminderID")]int id, [Name("Time")]DateTime time)
        {
            _reminderService.UpdateReminder(Context.Guild.Id, id, time: time);
            await ReplyAsync("Ok!");
        }

        [Command("UpdateType", RunMode = RunMode.Async)]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        public async Task UpdateReminderType([Name("ReminderID")]int id, [Name("Type")]ReminderType type)
        {
            _reminderService.UpdateReminder(Context.Guild.Id, id, type: type);
            await ReplyAsync("Ok!");
        }

        /// <summary>
        /// Dismisses a reminder
        /// </summary>
        /// <returns></returns>
        [Command("DismissReminder", RunMode = RunMode.Async)]
        [Alias("Dismiss", "End")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Dismisses a reminder for this server.")]
        public async Task DismissReminder([Name("ReminderID")] int id)
        {
            _reminderService.RemoveReminder(Context.Guild.Id, id);
            await ReplyAsync("Ok!");
        }

        /// <summary>
        /// Gets a reminder by the given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Command("GetReminder", RunMode = RunMode.Async)]
        [Alias("Get")]
        [RequireContext(ContextType.Guild)]
        [Summary("Gets a reminder for this server.")]
        public async Task GetReminderById([Name("ReminderID")]int id)
        {
            //var reminder = _reminderService.ActiveReminders.Find(x => x.GuildId == Context.Guild.Id && x.ID == id);
            var reminder = _reminderService.GetReminder(Context.Guild.Id, id);

            if (reminder == null)
            {
                await ReplyAsync("I couldn't find any active reminders by the supplied ID.");
            }
            else
            {
                var builder = new EmbedBuilder();
                builder.WithAuthor(Context.Client.CurrentUser);
                builder.WithColor(new Color(255, 204, 77));

                builder.WithTitle(string.Format("Reminder #{0}", reminder.ID));
                builder.WithCurrentTimestamp();
                builder.WithFooter("Type: " + reminder.ReminderType);
                string timealerts = "";
                foreach (var ts in reminder.ReminderTimeSpans)
                    timealerts += ts.ToString() + "\n";

                string description = string.Format(
                    "**{0:g}**\n{1}\n\nRemaining Alerts:\n{2}",
                    reminder.ReminderTime, reminder.ReminderText, timealerts);

                builder.WithDescription(description);
                await ReplyAsync("", false, builder.Build());
            }
        }

        /// <summary>
        /// Lists all of the guild reminders
        /// </summary>
        /// <returns></returns>
        [Command("GuildReminders", RunMode = RunMode.Async)]
        [Alias("ServerReminders", "ListServer", "ListGuild")]
        [RequireContext(ContextType.Guild)]
        [Summary("Gets all the reminders created for this server.")]
        public async Task ListGuildReminders()
        {
            var guildReminders = _reminderService.GetReminderGuild(Context.Guild.Id);

            var builder = new EmbedBuilder();
            builder.WithAuthor(Context.Client.CurrentUser);
            builder.WithColor(new Color(255, 204, 77));

            builder.WithTitle(string.Format("Reminders for {0}:", Context.Guild.Name));

            foreach(Reminder x in guildReminders)
            {
                var channel = await Context.Guild.GetChannelAsync(x.TextChannelId);
                var user = await Context.Guild.GetUserAsync(x.AuthorId);

                string descriptionText = string.Format("{0} {1}: `#{3}` {2}", channel.Name, user.Username ?? user.Nickname, x.ReminderText, x.ID);

                builder.AddField(x.ReminderTime.ToString("g"), descriptionText, true);
            }

            await ReplyAsync("", false, builder.Build());
        }

        /// <summary>
        /// Lists all of the channel reminders
        /// </summary>
        /// <returns></returns>
        [Command("ChannelReminders", RunMode = RunMode.Async)]
        [Alias("Reminders", "ListChannel")]
        [RequireContext(ContextType.Guild)]
        [Summary("Gets all the reminders created for the current channel, or specified channel.")]
        public async Task ListChannelReminders(IGuildChannel channel = null)
        {
            if (channel == null)
                channel = Context.Channel as IGuildChannel;

            // match all by AuthorId in this guild
            var channelReminders = _reminderService.GetReminderChannel(channel.Id);

            var builder = new EmbedBuilder();
            builder.WithAuthor(Context.Client.CurrentUser);
            builder.WithColor(new Color(255, 204, 77));

            builder.WithTitle(string.Format("Reminders for {0}:", channel.Name));

            foreach(Reminder x in channelReminders)
            {
                var user = await Context.Guild.GetUserAsync(x.AuthorId);

                string descriptionText = string.Format("{0}: `#{2}` {1}", user.Username ?? user.Nickname, x.ReminderText, x.ID);

                builder.AddField(x.ReminderTime.ToString("g"), descriptionText, true);
            }

                await ReplyAsync("", false, builder.Build());
            
        }

        /// <summary>
        /// Lists all of the reminders authored by the given user (or Context.User)
        /// </summary>
        /// <returns></returns>
        [Command("UserReminders", RunMode = RunMode.Async)]
        [Alias("MyReminders", "ListUser", "list")]
        [RequireContext(ContextType.Guild)]
        [Summary("Gets all the reminders authored by the specified user, or yourself if unspecified.")]
        public async Task ListUserReminders(IGuildUser user = null)
        {
            if (user == null)
                user = Context.User as IGuildUser;


            // match all by AuthorId in this guild
            var userReminders = _reminderService.GetReminderAuthor(user.Id, Context.Guild.Id);

                var builder = new EmbedBuilder();
                builder.WithAuthor(Context.Client.CurrentUser);
                builder.WithColor(new Color(255, 204, 77));

                builder.WithTitle(string.Format("Reminders for {0}:", user.Nickname ?? user.Username));

            foreach(Reminder x in userReminders)
            {
                string descriptionText = string.Format("`#{0}` {1}", x.ID, x.ReminderText);

                builder.AddField(x.ReminderTime.ToString("g"), descriptionText, true);
            }
                await ReplyAsync("", false, builder.Build());
            
        }

    }
}
