using System;
using System.Collections.Generic;
using System.Text;

namespace CSSBot.Services.TheSpookening.Models
{
    // logs when users perform actions
    public class UsageLog
    {
        // single word string which represents the type of action, currently just magic strings
        public string ActionType { get; set; }

        public ulong UserId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
