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
            "\u1f47b",
            // skull
            "\u1f480",
            // spider web
            "\u1f578",
            // spider
            "\u1f577",
            // bat
            "\u1f987",
            // jack o lantern
            "\u1f383",
            // lightning bolt
            "\u26a1"
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

        [Name("Spook")]
        [Alias("Scare")]
        [Summary("Spooks a user.")]
        public async Task Spook([Name("User")]IGuildUser user)
        {
            if (CheckIfOctober())
            {
                string replyMessage = string.Format(
                    "\u1f480\u1f480\u1f480 Uh oh! \u1f480\u1f480\u1f480\n{0} has been spooked!",
                    user.Mention
                    );
                await user.ModifyAsync(x =>
                {
                    x.Nickname += GetRandomEmoji();
                });
            }
            else
            {
                await ReplyAsync("Try again in October!");
            }
        }
    }
}
