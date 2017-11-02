using System;
using System.Collections.Generic;
using System.Text;

namespace CSSBot.Models
{
    // use the database to log all the times that we start up the bot
    // also as a quick way to test that the bot is working
    class StartupEvent
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
    }
}
