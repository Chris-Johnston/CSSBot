using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSSBot.Services.TheSpookening.Models
{
    // logs when users perform actions
    public class UsageLog
    {
        [BsonId]
        public int Id { get; set; }
        // single word string which represents the type of action, currently just magic strings
        [BsonField]
        public string ActionType { get; set; }

        [BsonField]
        public ulong UserId { get; set; }
        [BsonField]
        public DateTimeOffset Timestamp { get; set; }
    }
}
