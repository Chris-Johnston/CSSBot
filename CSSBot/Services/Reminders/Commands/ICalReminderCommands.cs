using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot.Reminders
{
    [Group("")]
    public class ICalReminderCommands : ModuleBase
    {
        private readonly ReminderService _reminders;

        public ICalReminderCommands(ReminderService reminders)
        {
            _reminders = reminders;
        }

        [Command("AddICal")]
        public async Task AddICalUrl([Remainder]string url)
        {
            _reminders.AddICalReminder(url, Context.Channel.Id, Context.Guild.Id);
            await ReplyAsync("OK added " + url);
        }
    }
}
