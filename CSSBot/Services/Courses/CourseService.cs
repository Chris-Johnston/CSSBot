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
            var m = await message.GetOrDownloadAsync();
            var textChannel = channel as IGuildChannel;
            if (!FromCurrentUser(m))
                return;
            var role = GetRole(m, textChannel, reaction);
            if (role == null)
                return;
            // add the role to the user
            var user = client.GetGuild(textChannel.Guild.Id).GetUser(reaction.UserId);
            if (user == null)
                return;
            Console.WriteLine($"Removed {user} to role {role}");
            await user.RemoveRoleAsync(role);
        }

        private IRole GetRole(IUserMessage m, IGuildChannel textChannel, SocketReaction reaction)
        {   
            if (textChannel == null)
                return null;
            if (reaction.UserId == client.CurrentUser.Id)
                return null;
            if (reaction.Emote.Name != check.Name)
                return null;
            var roleId = TryParseRole(m.Content);
            if (roleId == null)
                return null;
            var role = client.GetGuild(textChannel.Guild.Id).GetRole(roleId.Value);
            if (role == null)
                return null;
            return role;
        }

        private async Task Client_ReactionAdded(Discord.Cacheable<Discord.IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var m = await message.GetOrDownloadAsync();
            var textChannel = channel as IGuildChannel;
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
