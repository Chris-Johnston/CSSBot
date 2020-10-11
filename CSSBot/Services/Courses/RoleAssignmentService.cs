using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger logger;
        public CourseService(DiscordSocketClient client, ILogger logger)
        {
            this.client = client;

            this.client.ReactionAdded += Client_ReactionAdded;
            this.client.ReactionRemoved += Client_ReactionRemoved;
            this.logger = logger;
        }

        private bool CheckBotIsMessageAuthor(IUserMessage message)
        {
            if (message == null)
            {
                return false;
            }

            if (message?.Author?.Id == null)
            {
                logger.LogWarning("Message had a null author.");
                return false;
            }

            if (client?.CurrentUser?.Id == null)
            {
                logger.LogWarning("Check for valid user had a null current user.");
                return false;
            }

            if (message.Author?.IsBot == true)
            {
                logger.LogInformation("Failed to add user to course, was a bot.");
                return false;
            }

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
            if (m == null)
            {
                logger.LogWarning($"Failed to download message with id {message.Id}");
            }
            var textChannel = channel as IGuildChannel;
            // only message from the bot
            if (!CheckBotIsMessageAuthor(m))
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
            // must be checkmark, copy this filter earlier on as a faster check
            if (reaction == null || reaction.Emote.Name != check.Name)
                return;

            // get the message
            var m = await message.GetOrDownloadAsync();
            if (m == null)
            {
                logger.LogWarning($"Failed to download message with id {message.Id}");
            }
            var textChannel = channel as IGuildChannel;
            if (textChannel == null)
            {
                logger.LogDebug($"Reaction added to channel that was not IGuildChannel. channel Id: {channel.Id} message ID: {message.Id}");
            }

            // ignore if it's from the bot
            if (!CheckBotIsMessageAuthor(m))
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
