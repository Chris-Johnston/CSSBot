using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot.Commands
{
    [Group("")]
    [RequireBotPermission(Discord.GuildPermission.ManageChannels)]
    [RequireUserPermission(Discord.ChannelPermission.ManageChannels)]
    [RequireContext(ContextType.Guild)]
    public class CourseChannelModerationCommands : ModuleBase
    {
        [Command("NukeAndRebuildCurrentChannel")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireContext(ContextType.Guild)]
        // these parameters are added for a sanity check
        public async Task NukeAndRebuildCurrentChannel(ITextChannel textChannel, ulong textChannelId, string textChannelName)
        {
            // ensure that the parameters match the context
            if (Context.Channel.Id != textChannel.Id || Context.Channel.Id != textChannelId)
            {
                await ReplyAsync("Text channel Id did not match.");
                return;
            }

            if (Context.Channel.Name.ToLower() != textChannelName.ToLower())
            {
                await ReplyAsync("Text channel name did not match.");
                return;
            }

            var channel = Context.Channel as SocketTextChannel;
            await ReplyAsync("RIP this channel.");
            await channel.TriggerTypingAsync();
            // create a new channel with same properties
            var newChannel = await Context.Guild.CreateTextChannelAsync(channel.Name, x =>
            {
                x.CategoryId = channel.CategoryId;
                x.IsNsfw = channel.IsNsfw;
                x.Position = channel.Position;
                x.SlowModeInterval = channel.SlowModeInterval;
                x.Topic = channel.Topic;
            });

            // copy over permissions
            foreach (var permission in channel.PermissionOverwrites)
            {
                if (permission.TargetType == PermissionTarget.Role)
                {
                    // get permissions for role
                    var role = Context.Guild.GetRole(permission.TargetId);
                    await newChannel.AddPermissionOverwriteAsync(role, permission.Permissions);
                }
                else
                {
                    // get permissions for user
                    var user = await Context.Guild.GetUserAsync(permission.TargetId);
                    await newChannel.AddPermissionOverwriteAsync(user, permission.Permissions);
                }
            }

            // copy over webhooks
            foreach (var webhook in await channel.GetWebhooksAsync())
            {
                await webhook.ModifyAsync(x => x.ChannelId = newChannel.Id);
            }

            // nuke the old channel
            await channel.DeleteAsync();

            await newChannel.SendMessageAsync($"This channel was reset at {DateTime.UtcNow} UTC.");
        }

        /// <summary>
        /// Creates channels and roles for courses.
        /// </summary>
        [Command("CreateChannelAndRole", RunMode = RunMode.Async)]
        public async Task CreateChannelsAndRole(ICategoryChannel channelCategory, string courseName)
        {
            var category = channelCategory as SocketCategoryChannel;
            if (category == null)
            {
                await ReplyAsync("Invalid channel type. Must be a category channel.");
                return;
            }

            courseName = courseName.ToLower();
            var roleName = $"member_{channelCategory.Name}_{courseName}".ToLower();

            // ack
            var ackMessage = await ReplyAsync($"Ok, creating channel and role {courseName} under {category}");

            // check that a channel with the same name does not already exist
            if (category.Channels.Any(x => x.Name.ToLower() == courseName.ToLower()))
            {
                await ReplyAsync("Duplicate text channel under this category already exists. Quitting.");
                return;
            }

            var channel = await Context.Guild.CreateTextChannelAsync(courseName, x =>
            {
                x.CategoryId = channelCategory.Id;
                x.Topic = $"Course channel for {courseName}";
            });

            // set the everyone role for channel to disable view channel perm by default
            await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(viewChannel: PermValue.Deny));

            await ackMessage.ModifyAsync(x => x.Content = ackMessage.Content + $"\nCreated {channel}.");

            // check duplicate role
            if (Context.Guild.Roles.Any(x => x.Name.ToLower() == roleName))
            {
                await ReplyAsync("Duplicate role name found. Quitting.");
                return;
            }

            // create role
            var role = await Context.Guild.CreateRoleAsync(roleName, permissions: GuildPermissions.None);
            await role.ModifyAsync(x => x.Mentionable = true);

            await ackMessage.ModifyAsync(x => x.Content = ackMessage.Content + $"\nCreated role {role}.");

            var perms = new OverwritePermissions(viewChannel: PermValue.Allow);

            // assign role perms
            await channel.AddPermissionOverwriteAsync(role, perms);

            await ackMessage.ModifyAsync(x => x.Content = ackMessage.Content + $"\nAssigned perms to {role}.\nDone.");
        }

        [Command("sort")]
        public async Task SortCategory(ICategoryChannel channelCategory)
        {
            var category = channelCategory as SocketCategoryChannel;
            if (category == null)
            {
                await ReplyAsync("invalid channel cat");
                return;
            }

            int order = 1;

            var ackMsg = await ReplyAsync("k.");

            foreach (var channel in category.Channels.Where(x => x is INestedChannel).OrderBy(x => x.Name))
            {
                var c = channel as SocketGuildChannel;
                await c.ModifyAsync(x => x.Position = order);
                order++;
            }

            await ackMsg.ModifyAsync(x => x.Content = "k done.");
        }

        [Command("postjoinrole")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task PostJoinRoleMessage(IRole role)
        {
            var check = new Emoji("\u2705");
            var message = await ReplyAsync($"{role.Id}\nReact with a {check} on this message to get the `@{role}` role.");
            await message.AddReactionAsync(check);
        }

        [Command("postjoincourse", RunMode = RunMode.Async)]
        public async Task PostJoinMessages(ICategoryChannel channelCategory, params string[] courseCodes)
        {
            var category = channelCategory as SocketCategoryChannel;
            if (category == null)
            {
                await ReplyAsync("Invalid category channel id");
                return;
            }

            var message = await ReplyAsync("Starting.");

            foreach (var code in courseCodes)
            {
                var roleName = $"member_{channelCategory.Name}_{code}".ToLower();
                var channelName = code.ToLower();

                var channel = category.Channels.FirstOrDefault(x => x.Name == channelName);
                if (channel == null)
                {
                    await message.ModifyAsync(x => x.Content = message.Content + $"\nCouldn't find channel under {category} with name {channelName}");
                    continue;
                }
                var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == roleName);
                if (role == null)
                {
                    await message.ModifyAsync(x => x.Content = message.Content + $"\nCouldn't find role with name {roleName}");
                    continue;
                }

                await PostJoinMessage(channel, role, code, channelCategory);
            }
        }

        private async Task PostJoinMessage(IGuildChannel classChannel, IRole courseRole, string course, ICategoryChannel category)
        {
            var check = new Emoji("\u2705");
            var message = await ReplyAsync($"{courseRole.Id}\nReact with a {check} on this message to get the {courseRole} role, and join the {classChannel} channel.");
            await message.AddReactionAsync(check);
        }
    }
}
