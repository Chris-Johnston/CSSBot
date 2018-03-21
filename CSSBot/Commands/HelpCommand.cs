using Discord.Commands;
using Discord.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot
{
    [Group("")]
    public class HelpCommand : ModuleBase
    {
        private readonly CommandService m_commands;
        public HelpCommand(CommandService commands)
        {
            m_commands = commands;
        }

        [Command("Help")]
        [Summary("Replies back with help text.")]
        [RequireBotPermission(Discord.GuildPermission.SendMessages)]
        [RequireContext(ContextType.Guild)]
        [Priority(2)]
        public async Task HelpGuild()
        {
            try
            {
                var channel = await Context.User.GetOrCreateDMChannelAsync();
                await channel.SendMessageAsync(HelpText(false));
            }
            catch (HttpException)
            {
                await ReplyAsync(HelpText(false));
            }

        }

        [Command("HelpHere")]
        [Summary("Replies back with help text in current context.")]
        public async Task HelpHere()
        {
            await ReplyAsync(HelpText(false));
        }

        [Command("Help")]
        [Summary("Replies back with help text.")]
        [RequireContext(ContextType.Guild)]
        [Priority(1)]
        public async Task HelpGuildDMToUser()
        {
            var channel = await Context.User.GetOrCreateDMChannelAsync();
            await channel.SendMessageAsync(HelpText(false));
        }

        [Command("Help")]
        [Summary("Replies back with help text.")]
        [RequireContext(ContextType.DM)]
        [Priority(1)]
        public async Task HelpDM()
        {
            await ReplyAsync(HelpText(false));
        }

        public string HelpText(bool showOwner = false)
        {
            string output = "**Help:**\n";
            foreach (var module in m_commands.Modules.OrderBy(x => x.Name))
            {
                GetModuleInformation(module, ref output, showOwner);
            }
            return output;
        }

        public void GetCommandInformation(CommandInfo command, ref string output, bool showOwner = false)
        {
            // checks if it should include this module if it is for the owner only
            if (!showOwner)
            {
                //bool containsOwnerAttribute = false;
                foreach (var atr in command.Attributes)
                {
                    if (atr is RequireOwnerAttribute)
                    {
                        //containsOwnerAttribute = true;
                        // if this attribute is found then just return
                        return;
                    }
                }
            }

            // add the command info
            output += $"{command.Name} {GetParameters(command)}";
        }

        public string GetParameters(CommandInfo command)
        {
            string result = "";
            foreach (var p in command.Parameters)
            {
                if (p.IsOptional)
                {
                    result += $"({p.Name} = {p.DefaultValue ?? "default"})";
                }
                else if (p.IsMultiple)
                {
                    result += $"[{p.Name}]";
                }
                else if (p.IsRemainder)
                {
                    result += $"{p.Name}...";
                }
                else
                {
                    result += $"<{p.Name}>";
                }
                result += " ";
            }
            return result;
        }

        public void GetModuleInformation(ModuleInfo module, ref string output, bool showOwner = false)
        {
            // checks if it should include this module if it is for the owner only
            if (!showOwner)
            {
                //bool containsOwnerAttribute = false;
                foreach (var atr in module.Attributes)
                {
                    if (atr is RequireOwnerAttribute)
                    {
                        //containsOwnerAttribute = true;
                        // if this attribute is found then just return
                        return;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(module.Name))
            {
                // add a header for the module
                output += $"\nModule {module.Name.Normalize()}: {((module.Aliases.Count > 1) ? string.Empty : "(" + string.Join(",", module.Aliases) + ")")}";
            }

            // add each of the commands
            var commands = new List<CommandInfo>(module.Commands);
            var grouping = commands.OrderBy(x => x.Name).GroupBy(x => x.Name, x => x);
            foreach (var group in grouping)
            {
                var command = group.FirstOrDefault();
                output += $"\n{GlobalConfiguration.CommandPrefix}{((!string.IsNullOrWhiteSpace(module.Name)) ? module.Name + " " : string.Empty)}";
                GetCommandInformation(command, ref output, showOwner);
            }
        }
    }
}
