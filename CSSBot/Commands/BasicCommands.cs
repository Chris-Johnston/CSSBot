using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot.Commands
{
    /// <summary>
    /// Just some basic commands to get started
    /// </summary>
    public class BasicCommands : ModuleBase
    {
        /// <summary>
        /// Basic Ping/Pong command
        /// </summary>
        /// <returns></returns>
        [Command("Ping")]
        public async Task Ping()
        {
            await ReplyAsync("Pong!");
        }
        
        /// <summary>
        /// Help text command
        /// </summary>
        /// <returns></returns>
        [Command("Help")]
        public async Task Help()
        {
            await ReplyAsync("todo");
        }

        /// <summary>
        /// About text command
        /// Includes a link to the repo
        /// </summary>
        /// <returns></returns>
        [Command("About")]
        [Alias("GitHub")]
        public async Task About()
        {
            string txt = string.Format("🤔 CSSBot 🤔\nGitHub: https://github.com/Chris-Johnston/CSSBot");
            await ReplyAsync(txt);
        }

    }
}
