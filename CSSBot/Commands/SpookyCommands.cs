using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot.Commands
{
    public class SpookyCommands : ModuleBase
    {
        // this previously contained many commands for user nickname manipulation,
        // but that got really messy quick and turned out to be a bad idea

        /// <summary>
        /// Adds the skull and trumpet emoji as a reaction to the user's
        /// command message and replies back with the youtube link for
        /// the skull trumpet video
        /// </summary>
        /// <returns></returns>
        [Command("Doot")]
        [Alias("SkullTrumpet")]
        [RequireUserPermission(GuildPermission.ReadMessages | GuildPermission.SendMessages)]
        public async Task Doot()
        {
            // if october
            if (DateTime.Now.Month == 10)
            {
                await Context.Message.AddReactionAsync(new Emoji("💀"));
                await Context.Message.AddReactionAsync(new Emoji("🎺"));

                await ReplyAsync(@"https://www.youtube.com/watch?v=eVrYbKBrI7o");
            }
        }
    }
}
