using Discord.WebSocket;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSSBot.Tags
{
    public class TagNotFoundException : Exception
    {
        public TagNotFoundException() : base() { }
        public TagNotFoundException(string msg) : base(msg) { }
    }

    public class DuplicateTagFoundException : Exception
    {
        public Tag Duplicate { get; private set; }
        public DuplicateTagFoundException(Tag duplicate) : base() { Duplicate = duplicate; }
    }

    public class BannedTagUserException : Exception
    {
        public ulong UserId { get; private set; }
        public BannedTagUserException(ulong userId) : base() { UserId = userId; }
    }

    public class TagService
    {
        private readonly string TagTable = "Tags";
        private readonly string TagBanTable = "TagBans";

        private readonly LiteDatabase _db;

        public TagService(LiteDatabase db)
        {
            _db = db;
        }

        /// <summary>
        /// Gets the tag by the given text for the given author, or by the guild
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="guildId"></param>
        /// <param name="channelId"></param>
        /// <param name="authorId"></param>
        /// <returns></returns>
        public Tag GetTag(string tagName, ulong guildId, ulong authorId)
        {
            // when searching for a tag, ensure that tags are only for users inside a guild
            // try to match by text for that author
            // if no matches, then try to match by text in that server
            // otherwise, see if can match by similar (dunno if supported by litedb, and I don't care that much)

            string norm = NormalizeTagStr(tagName);

            var tag = _db.GetCollection<Tag>(TagTable)
                .FindOne(x => x.TagKey == norm && x.GuildId == guildId && x.AuthorId == authorId);

            if (tag != null) return tag;

            // search entire guild
            tag = _db.GetCollection<Tag>(TagTable)
                .FindOne(x => x.TagKey == norm && x.GuildId == guildId);

            if (tag != null) return tag;

            // not found
            throw new TagNotFoundException($"The tag `{norm}` was not found.");
        }

        /// <summary>
        /// Gets all of the tags for the given user in the guild
        /// </summary>
        /// <param name="author"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public IEnumerable<Tag> GetUserTags(ulong author, ulong guildId)
        {
            return _db.GetCollection<Tag>(TagTable).Find(x => x.AuthorId == author && x.GuildId == guildId);
        }

        /// <summary>
        /// Gets all of the tags for the guild
        /// </summary>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public IEnumerable<Tag> GetGuildTags(ulong guildId)
        {
            return _db.GetCollection<Tag>(TagTable).Find(x => x.GuildId == guildId);
        }

        /// <summary>
        /// Makes a new tag and adds it to the database
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="contents"></param>
        /// <param name="guildId"></param>
        /// <param name="authorId"></param>
        /// <returns></returns>
        public void MakeTag(string tagName, string contents, ulong guildId, ulong authorId)
        {
            // check that this user has permission to make tags
            if(IsUserBanned(authorId, guildId))
            {
                throw new BannedTagUserException(authorId);
            }

            // check to see that a tag with this name by the given author in this guild doesn't already exist
            // (duplicate tag names are ok, since they are ranked by if they belong to the author or not)

            string norm = NormalizeTagStr(tagName);

            var duplicate = _db.GetCollection<Tag>(TagTable)
                .FindOne(x => x.AuthorId == authorId && x.GuildId == guildId && x.TagKey == norm);

            if (duplicate != null)
            {
                throw new DuplicateTagFoundException(duplicate);
            }

            // otherwise make the new tag
            var tag = new Tag()
            {
                GuildId = guildId,
                AuthorId = authorId,
                TagKey = norm,
                TagValue = contents,
                CreatedTime = DateTime.Now
            };

            // add the tag to the database
            _db.GetCollection<Tag>(TagTable).Insert(tag);
        }

        /// <summary>
        /// Removes all of the tags that match the name in the given guild
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="guildId"></param>
        /// <returns></returns>
        public void RemoveTagByName(string tagName, ulong guildId)
        {
            _db.GetCollection<Tag>(TagTable).Delete(x => x.TagKey == NormalizeTagStr(tagName) && x.GuildId == guildId);
        }

        /// <summary>
        /// Removes the tag by the given ID in the given guild
        /// </summary>
        /// <param name="tagId"></param>
        /// <param name="guildId"></param>
        public void RemoveTagById(int tagId, ulong guildId)
        {
            _db.GetCollection<Tag>(TagTable).Delete(x => x.Id == tagId && x.GuildId == guildId);
        }
        
        /// <summary>
        /// Removes all tags by a certain author
        /// </summary>
        /// <param name="authorId"></param>
        public void RemoveAllTagsByUser(ulong authorId, ulong guildId)
        {
            _db.GetCollection<Tag>(TagTable).Delete(x => x.GuildId == guildId && x.AuthorId == authorId );
        }

        /// <summary>
        /// Remove the user from the list of bans for the guild
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="guildId"></param>
        public void UnBanUser(ulong userid, ulong guildId)
        {
            _db.GetCollection<TagBannedUser>(TagBanTable).Delete(x => x.UserId == userid && x.GuildId == x.GuildId);
        }

        /// <summary>
        /// Add the user to the list of bans for the guild
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="guildId"></param>
        public void BanUser(ulong userId, ulong guildId)
        {
            TagBannedUser n = new TagBannedUser()
            {
                BanTime = DateTime.Now,
                UserId = userId,
                GuildId = guildId
            };

            _db.GetCollection<TagBannedUser>(TagBanTable).Insert(n);
        }

        public bool IsUserBanned(ulong userId, ulong guildId)
        {
            return _db.GetCollection<TagBannedUser>(TagBanTable).Exists(x => x.GuildId == guildId && x.UserId == userId);
        }

        private static string NormalizeTagStr(string s) => Discord.Format.Sanitize(s.Trim().ToLower());
    }
}
