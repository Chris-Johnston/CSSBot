using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSSBot.Reminders.Models
{
    /// <summary>
    /// Associates a ICal URL to a text channel so that it can be polled and notifications can be sent to the channel.
    /// </summary>
    public class ReminderICalSource
    {
        public ReminderICalSource() { }

        [BsonId]
        public int Id { get; set; }

        /// <summary>
        /// Which text should see reminders for this calendar?
        /// </summary>
        [BsonField]
        public ulong TextChannelId { get; set; }

        /// <summary>
        /// Which Guild contains the text channel that this calendar is for?
        /// </summary>
        [BsonField]
        public ulong GuildId { get; set; }

        /// <summary>
        /// Http(s) URL to a publicly accessible .ical file
        /// This should be checked when set to ensure the protocol and the correct destination.
        /// </summary>
        [BsonField]
        public Uri ICalPath { get; set; }
    }
}
