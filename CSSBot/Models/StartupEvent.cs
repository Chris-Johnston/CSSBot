using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSSBot.Models
{
    // use the database to log all the times that we start up the bot
    // also as a quick way to test that the bot is working
    class StartupEvent
    {
        [BsonId]
        public int Id { get; set; }
        [BsonField]
        public DateTime Time { get; set; }
    }
}
