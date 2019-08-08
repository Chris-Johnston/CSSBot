using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot.Services.Courses
{
    public class CourseService
    {
        private readonly DiscordSocketClient client;
        private readonly Emoji check = new Emoji("\u2705");
        public CourseService(DiscordSocketClient client)
        {
            this.client = client;

            this.client.ReactionAdded += Client_ReactionAdded;
            this.client.ReactionRemoved += Client_ReactionRemoved;
        }

        private bool FromCurrentUser(IUserMessage message)
        {
            return message.Author.Id == client.CurrentUser.Id;
        }

        private ulong? TryParseRole(string content)
        {
            var split = content.Split('\n');
            if (split.Length > 1)
            {
                var roleStr = split[0];
                if (ulong.TryParse(roleStr, out var result))
                    return result;
            }
            return null;
        }

        private async Task Client_ReactionRemoved(Discord.Cacheable<Discord.IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            // get message, check conditions
            var m = await message.GetOrDownloadAsync();
            var textChannel = channel as IGuildChannel;
            // only message from the bot
            if (!FromCurrentUser(m))
                return;
            // get the role from the first part of the message
            var role = GetRole(m, textChannel, reaction);
            if (role == null)
                return;
            // add the role to the user
            var user = client.GetGuild(textChannel.Guild.Id).GetUser(reaction.UserId);
            if (user == null)
                return;
            // remove role
            await user.RemoveRoleAsync(role);
        }

        private IRole GetRole(IUserMessage m, IGuildChannel textChannel, SocketReaction reaction)
        {   
            // must be IGuildChannel
            if (textChannel == null)
                return null;
            // ignore reactions from the bot
            if (reaction.UserId == client.CurrentUser.Id)
                return null;
            // must be checkmark
            if (reaction.Emote.Name != check.Name)
                return null;
            // get the role from the message
            var roleId = TryParseRole(m.Content);
            if (roleId == null)
                return null;
            // ensure role exists on the guild
            var role = client.GetGuild(textChannel.Guild.Id).GetRole(roleId.Value);
            if (role == null)
                return null;
            return role;
        }

        private async Task Client_ReactionAdded(Discord.Cacheable<Discord.IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            // get the message
            var m = await message.GetOrDownloadAsync();
            var textChannel = channel as IGuildChannel;
            // only from the bot
            if (!FromCurrentUser(m))
                return;
            var role = GetRole(m, textChannel, reaction);
            if (role == null)
                return;
            // add the role to the user
            var user = client.GetGuild(textChannel.Guild.Id).GetUser(reaction.UserId);
            if (user == null)
                return;
            Console.WriteLine($"Added {user} to role {role}");
            await user.AddRoleAsync(role);
        }
    }
}
