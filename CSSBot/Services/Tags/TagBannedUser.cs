using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSSBot.Tags
{
    public class TagBannedUser
    {
        public TagBannedUser() { }

        [BsonId]
        public int Id { get; set; }

        [BsonField]
        public DateTime BanTime { get; set; }

        [BsonField]
        public ulong UserId { get; set; }

        [BsonField]
        public ulong GuildId { get; set; }
    }
}
