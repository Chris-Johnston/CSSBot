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
            var added = _reminderService.AddReminder(Context.Guild.Id, Context.Channel.Id, Context.User.Id,
                ReminderText, reminderTime);

            if (added != null)
            {
                string replyText = string.Format("Ok {0}! I've created a reminder for `{1:g}` with the ID # of {2}.", Context.User.Mention, reminderTime, added.ReminderId);
                await ReplyAsync(replyText);
            }
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
            // only allow the author of the reminder, or a user with ManageMessages or Administrator
            // to remove messages
            var reminder = _reminderService.ActiveReminders.Find(x => x.GuildId == Context.Guild.Id && x.ReminderId == id);

            if (reminder == null)
            {
                await ReplyAsync("I couldn't find any active reminders by the supplied ID.");
            }
            else
            {
                _reminderService.RemoveReminderById(Context.Guild.Id, reminder.ReminderId);
                await ReplyAsync("Reminder deleted.");
            }
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
            var reminder = _reminderService.ActiveReminders.Find(x => x.GuildId == Context.Guild.Id && x.ReminderId == id);

            if(reminder == null)
            {
                await ReplyAsync("I couldn't find any active reminders by the supplied ID.");
            }
            else
            {
                var builder = new EmbedBuilder();
                builder.WithAuthor(Context.Client.CurrentUser);
                builder.WithColor(new Color(255, 204, 77));

                builder.WithTitle(string.Format("Reminder #{0}", reminder.ReminderId));

                builder.AddField(reminder.ReminderTime.ToString("g"),
                    string.Format("{0}\n\nRemaining alerts: {1}\nType: {2}", reminder.ReminderText, reminder.ReminderTimeOption, reminder.ReminderType));

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
            // match all by AuthorId in this guild
            var guildReminders = _reminderService.ActiveReminders.FindAll(x => x.GuildId == Context.Guild.Id);

            if (guildReminders.Count == 0)
            {
                await ReplyAsync("There are no active reminders for this server.");
            }
            else
            {
                var builder = new EmbedBuilder();
                builder.WithAuthor(Context.Client.CurrentUser);
                builder.WithColor(new Color(255, 204, 77));

                builder.WithTitle(string.Format("Reminders for {0}:", Context.Guild.Name));

                guildReminders.ForEach(async x =>
                {
                    var channel = await Context.Guild.GetChannelAsync(x.TextChannelId);
                    var user = await Context.Guild.GetUserAsync(x.AuthorId);

                    string descriptionText = string.Format("{0} {1}: #{3} {2}", channel.Name, user.Username ?? user.Nickname, x.ReminderText, x.ReminderId);

                    builder.AddField(x.ReminderTime.ToString("g"), descriptionText, true);
                });

                await ReplyAsync("", false, builder.Build());
            }
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
            var channelReminders = _reminderService.ActiveReminders.FindAll(x => x.TextChannelId == channel.Id && x.GuildId == channel.GuildId);

            if (channelReminders.Count == 0)
            {
                await ReplyAsync(string.Format("There are no active reminders for the channel {0}.", channel));
            }
            else
            {
                var builder = new EmbedBuilder();
                builder.WithAuthor(Context.Client.CurrentUser);
                builder.WithColor(new Color(255, 204, 77));

                builder.WithTitle(string.Format("Reminders for {0}:", channel.Name));

                channelReminders.ForEach(async x =>
                {
                    var user = await Context.Guild.GetUserAsync(x.AuthorId);

                    string descriptionText = string.Format("{0}: #{2} {1}", user.Username ?? user.Nickname, x.ReminderText, x.ReminderId);

                    builder.AddField(x.ReminderTime.ToString("g"), descriptionText, true);
                });

                await ReplyAsync("", false, builder.Build());
            }
        }

        /// <summary>
        /// Lists all of the reminders authored by the given user (or Context.User)
        /// </summary>
        /// <returns></returns>
        [Command("UserReminders", RunMode = RunMode.Async)]
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
                   string descriptionText = string.Format("#{0} {1}", x.ReminderId, x.ReminderText);

                   builder.AddField(x.ReminderTime.ToString("g"), descriptionText, true);
               });

                await ReplyAsync("", false, builder.Build());
            }
        }

    }
}
