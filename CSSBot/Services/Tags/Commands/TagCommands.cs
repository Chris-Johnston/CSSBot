using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot.Tags
{
    [Group("")]
    public class TagCommands : ModuleBase
    {
        private readonly TagService _tags;

        public TagCommands(TagService tags)
        {
            _tags = tags;
        }

        /// <summary>
        /// Gets the contents of a tag if it is found in a channel
        /// </summary>
        /// <returns></returns>
        [Command("tag")]
        [Summary("Gets the contents of a tag if it exists.")]
        [RequireUserPermission(Discord.GuildPermission.SendMessages)]
        [RequireBotPermission(GuildPermission.SendMessages)]
        [RequireContext(ContextType.Guild)]
        public async Task GetTag(string tagName)
        {
            try
            {
                var t = _tags.GetTag(tagName, Context.Guild.Id, Context.User.Id);
                await ReplyAsync(t.ToString());
            }
            catch (TagNotFoundException e)
            {
                await ReplyAsync(e.Message);
            }
        }

        [Command("maketag")]
        [Summary("Makes a new tag.")]
        [RequireUserPermission(Discord.GuildPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.AddReactions | ChannelPermission.SendMessages)]
        [RequireContext(ContextType.Guild)]
        public async Task MakeTag(string tag, [Remainder]string contents)
        {
            try
            {
                _tags.MakeTag(tag, contents, Context.Guild.Id, Context.User.Id);

                await Context.Message.AddReactionAsync(new Emoji("🆗"));
            }
            catch (BannedTagUserException e)
            {
                await Context.Message.AddReactionAsync(new Emoji("🔨"));
            }
            catch (DuplicateTagFoundException e)
            {
                await ReplyAsync($"You've already made a tag with this value before.");
            }
        }

        [Command("deletetag")]
        [Alias("deltag")]
        [Summary("Deletes a tag by Id.")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ReadMessages | GuildPermission.AddReactions)]
        [RequireContext(ContextType.Guild)]
        public async Task DeleteTag(int id)
        {
            // remove the tag
            _tags.RemoveTagById(id, Context.Guild.Id);
            // ack to the user
            await Context.Message.AddReactionAsync(new Emoji("🆗"));
        }

        [Command("bantag")]
        [Alias("tagban")]
        [Summary("Bans a user from using the tag system.")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ReadMessages | GuildPermission.AddReactions)]
        [RequireContext(ContextType.Guild)]
        public async Task BanTag(IGuildUser user)
        {
            // cannot ban users that are admin
            if (user.GuildPermissions.ManageMessages | user.GuildPermissions.Administrator) 
            {
                await ReplyAsync("Cannot ban users with the 'Manage Messages' permission.");
                return;
            }
            
            // ban the user 
            _tags.BanUser(user.Id, Context.Guild.Id);
            // remove the tags by the user
            _tags.RemoveAllTagsByUser(user.Id, Context.Guild.Id);
            // ack to the user
            await Context.Message.AddReactionAsync(new Emoji("🔨"));
        }

        [Command("unbantag")]
        [Alias("tagunban")]
        [Summary("Unbans a user from using the tag system.")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ReadMessages | GuildPermission.AddReactions)]
        [RequireContext(ContextType.Guild)]
        public async Task UnBanTag(IGuildUser user)
        {
            // unban the user
            _tags.UnBanUser(user.Id, Context.Guild.Id);
            // ack to the user
            await Context.Message.AddReactionAsync(new Emoji("🆗"));
        }

        [Command("taginfo")]
        [Summary("Gets info about a tag")]
        [RequireUserPermission(GuildPermission.SendMessages)]
        [RequireBotPermission(GuildPermission.SendMessages)]
        [RequireContext(ContextType.Guild)]
        public async Task TagInfo(string tagName)
        {
            try
            {
                var t = _tags.GetTag(tagName, Context.Guild.Id, Context.User.Id);
                await ReplyAsync(
                    $"`{t.TagKey}` (#{t.Id}) Created by user ID {t.AuthorId} at {t.CreatedTime.ToString("G")}"
                    );
            }
            catch (TagNotFoundException e)
            {
                await ReplyAsync(e.Message);
            }
        }
        
        [Command("taglist")]
        [Alias("listtags", "listtag")]
        [RequireUserPermission(GuildPermission.SendMessages)]
        [RequireBotPermission(GuildPermission.SendMessages)]
        [RequireContext(ContextType.Guild)]
        public async Task ListTags()
        {
            string tagstr = "Tags: ";
            foreach(var t in _tags.GetGuildTags(Context.Guild.Id))
            {
                tagstr += $"{t.TagKey} ({t.Id}), ";
            }
            await ReplyAsync(tagstr);
        }

        [Command("usertags")]
        [Alias("taguser")]
        [RequireUserPermission(GuildPermission.SendMessages)]
        [RequireBotPermission(GuildPermission.SendMessages)]
        [RequireContext(ContextType.Guild)]
        public async Task ListUserTags(IGuildUser user = null)
        {
            if (user == null)
                user = Context.User as IGuildUser;

            string tagstr = $"Tags for user {user.Username}#{user.Discriminator}: ";
            foreach (var t in _tags.GetUserTags(user.Id, Context.Guild.Id))
            {
                tagstr += $"{t.TagKey} ({t.Id}), ";
            }
            await ReplyAsync(tagstr);
        }

        //todo add a command for cleaning up previous tags being displayed
    }
}
