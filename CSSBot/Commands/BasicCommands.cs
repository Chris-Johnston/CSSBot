using Discord;
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
        // as far as I'm concerned, Dependency Injection is black magic
        // here are the docs I referenced to get this working
        // https://discord.foxbot.me/docs/guides/commands/commands.html#dependency-injection
        private readonly CommandService _commandService;

        public BasicCommands(CommandService commands)
        {
            _commandService = commands;
        }

        /// <summary>
        /// Basic Ping/Pong command
        /// </summary>
        /// <returns></returns>
        [Command("Ping"), Summary("A simple ping/pong command, for checking if the bot works.")]
        public async Task Ping()
        {
            await ReplyAsync("Pong!");
        }

        /// <summary>
        /// Echo command
        /// </summary>
        /// <returns></returns>
        [Command("Echo"), Summary("A simple echo command.")]
        public async Task Echo([Name("Text"), Summary("The text to echo back.")] string text)
        {
            await ReplyAsync(Context.User.Mention + " : " + text);
        }

        /// <summary>
        /// Help text command
        /// </summary>
        /// <returns></returns>
        [Command("Help"), Summary("Replies with some help text.")]
        public async Task Help()
        {
            var embed = new EmbedBuilder();
            embed.WithAuthor(Context.Client.CurrentUser as IUser);

            embed.WithColor(new Color(255, 204, 77));

            embed.WithTitle("Help");

            foreach(var x in _commandService.Commands)
            {
                embed.AddField(x.Name, GenerateCommandDescription(x), true);
            }

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        private string GenerateCommandDescription(CommandInfo command)
        {
            string ret = "`";
            // show how the command should be used
            ret += GlobalConfiguration.CommandPrefix + command.Name;

            // add parameters
            foreach(var par in command.Parameters)
            {
                ret += " ";
                // surround optional paramters with []
                // non optional with < >
                if (par.IsOptional)
                    ret += "[";
                else
                    ret += "<";

                ret += par.Type.Name + " " + par.Name;

                if (par.IsOptional)
                    ret += "]";
                else
                    ret += ">";
            }
            ret += "`\n" + command.Summary;

            return ret;
        }

        /// <summary>
        /// About text command
        /// Includes a link to the repo
        /// </summary>
        /// <returns></returns>
        [Command("About"), Summary("Replies back with some help text.")]
        [Alias("GitHub")]
        public async Task About()
        {
            string txt = string.Format("🤔 CSSBot 🤔\nGitHub: https://github.com/Chris-Johnston/CSSBot");
            await ReplyAsync(txt);
        }

    }
}
