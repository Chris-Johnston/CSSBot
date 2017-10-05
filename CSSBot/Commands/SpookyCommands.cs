using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot.Commands
{
    //todo don't load this automatically but only load it if in october
    public class SpookyCommands : ModuleBase
    {
        private string[] _HalloweenEmoji = new string[]
        {
            // ghost
            "👻",
            // skull
            "💀",
            // spider web
            "🕸️",
            // spider
            "🕷️",
            // bat
            "🦇",
            // jack o lantern
            "🎃",
            // lightning bolt
            "⚡"
        };

        private string GetRandomEmoji()
        {
            Random r = new Random();
            return _HalloweenEmoji[r.Next(_HalloweenEmoji.Length)];
        }

        private bool CheckIfOctober()
        {
            return DateTime.Now.Month == 10;
        }

        [Command("Spook")]
        [Alias("Scare")]
        [Summary("Spooks a user.")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.ManageNicknames)]
        public async Task Spook([Name("User")]IGuildUser user)
        {
            foreach(string s in _HalloweenEmoji)
            {
                Console.WriteLine(s);
            }

            if (CheckIfOctober())
            {
                string replyMessage = string.Format(
                    "💀💀💀 Uh oh! 💀💀💀\n\n{0} has been spooked!",
                    user.Mention
                    );
                await user.ModifyAsync(x =>
                {
                    x.Nickname += (user.Nickname ?? user.Username) + GetRandomEmoji();
                });

                await ReplyAsync(replyMessage);
            }
            else
            {
                await ReplyAsync("Try again in October!");
            }
        }
    }
}
