using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot.Commands
{
    // commands for administration by the bot owner
    [Group("Admin")]
    [RequireOwner()]
    public class AdminCommands : ModuleBase
    {
        [Command("AnnounceGuildChannel")]
        [RequireContext(ContextType.DM)]
        [RequireOwner()]
        public async Task AnnounceToGuildChannel(ulong guildId, ulong textChannelID, [Remainder] string text)
        {
            var guild = await Context.Client.GetGuildAsync(guildId) as SocketGuild;
            var channel = guild.GetTextChannel(textChannelID) as SocketTextChannel;

            var embed = new EmbedBuilder();
            embed.WithColor(new Color(255, 204, 77));
            embed.WithTitle("Announcement");
            embed.WithCurrentTimestamp();
            embed.WithAuthor(Context.User);
            embed.WithDescription(text);

            var result = await channel.SendMessageAsync("", false, embed.Build());
            await ReplyAsync("OK, sent.");
            await ReplyAsync(result.Id.ToString(), false, embed.Build());
        }

        [Command("SendMessageGuildChannel")]
        [RequireContext(ContextType.DM)]
        [RequireOwner()]
        public async Task SendMessageToGuildChannel(ulong guildId, ulong textChannelId, [Remainder] string text)
        {
            var guild = await Context.Client.GetGuildAsync(guildId) as SocketGuild;
            var channel = guild.GetTextChannel(textChannelId) as SocketTextChannel;

            var result = await channel.SendMessageAsync(text);
            await ReplyAsync("OK, " + result.Id + ".\n" + text);
        }

        [Command("SendMessageUser")]
        [RequireContext(ContextType.DM)]
        [RequireOwner()]
        public async Task SendMessageToUser(ulong userId, [Remainder] string text)
        {
            var user = await Context.Client.GetUserAsync(userId) as SocketUser;
            var channel = await user.GetOrCreateDMChannelAsync();

            var result = await channel.SendMessageAsync(text);
            await ReplyAsync("OK, " + result.Id + ".\n" + text);
        }

        [Command("GuildBanUserByID")]
        [RequireContext(ContextType.DM)]
        [RequireOwner()]
        public async Task GuildBanUserByID(ulong guildId, ulong userId)
        {
            var guild = await Context.Client.GetGuildAsync(guildId);
            await guild.AddBanAsync(userId);

            await ReplyAsync("Ok, banned user " + userId + " from guild " + guildId);
        }
    }
}
