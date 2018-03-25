using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSSBot.Tags
{
    /// <summary>
    /// Represents a tag, or a string of text that should be stored under some name
    /// </summary>
    public class Tag
    {
        public Tag() { }

        /// <summary>
        /// The unique tag ID
        /// </summary>
        [BsonId]
        public int Id { get; set; }

        /// <summary>
        /// The guild that this tag is associated to
        /// Tags must not transfer across guilds
        /// </summary>
        [BsonField]
        public ulong GuildId { get; set; }

        /// <summary>
        /// The user id who authored this tag
        /// </summary>
        [BsonField]
        public ulong AuthorId { get; set; }

        /// <summary>
        /// The name of this tag, should be a single string without spaces
        /// </summary>
        [BsonField]
        public string TagKey { get; set; }

        /// <summary>
        /// The contents of the tag, a string of unspecified length
        /// </summary>
        [BsonField]
        public string TagValue { get; set; }

        /// <summary>
        /// When the tag was created
        /// </summary>
        [BsonField]
        public DateTime CreatedTime { get; set; }

        [BsonIgnore]
        public override string ToString()
        {
            return $"{TagKey} ({Id}): {TagValue}";
        }
    }
}
