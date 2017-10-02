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
    /// use an empty name attribute because these are default
    /// </summary>
    [Name("")]
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
        public async Task Echo([Name("Text"), Summary("The text to echo back."), Remainder] string text)
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
            if(string.IsNullOrWhiteSpace(command.Module.Name))
            {
                ret += GlobalConfiguration.CommandPrefix + command.Name;
            }
            else
            {
                ret += string.Format("{0}{1} {2}", GlobalConfiguration.CommandPrefix, command.Module.Name, command.Name);
            }

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

        [Command("Cleanup", RunMode = RunMode.Async)]
        [Summary("Deletes a specified number of the bot's recent messages.")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireContext(ContextType.Guild)]
        public async Task CleanupRecent([Name("NumMsg")] int amountToCleanup = 10)
        {
            // ensure bounds
            // don't allow deleting more than 25 because that's a lot to delete
            // and we don't want to spam api either
            if (amountToCleanup < 0 || amountToCleanup > 25) amountToCleanup = 10;
            foreach( var message in await Context.Channel.GetMessagesAsync(Context.Message.Id, Direction.Before, amountToCleanup).Flatten())
            {
                if(message.Author.Id == Context.Client.CurrentUser.Id)
                    await message.DeleteAsync();
            }
        }

    }
}
