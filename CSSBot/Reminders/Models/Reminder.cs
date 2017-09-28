using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CSSBot.Reminders
{
    /// <summary>
    /// Represents a reminder, which will be saved/loaded from a file
    /// Need to use a file so that reminders aren't lost when bot restarts
    /// </summary>
    public class Reminder
    {
        // the unique id of this reminder
        [XmlElement("ReminderId")]
        public int ReminderId { get; set; }

        // the guild that created this reminder
        [XmlElement("GuildId")]
        public ulong GuildId { get; set; }

        // the text channel that this reminder is for
        [XmlElement("TextChannelId")]
        public ulong TextChannelId { get; set; }

        // the author of this reminder
        [XmlElement("AuthorId")]
        public ulong AuthorId { get; set; }

        // the text that makes up the reminder itself
        [XmlElement("ReminderText")]
        public string ReminderText { get; set; }

        // when the reminder is set to activate
        [XmlElement("ReminderTime")]
        public DateTime ReminderTime { get; set; }

        // which reminder time options were set upon creation
        // each bit is set to 1 if it still needs to be set
        // and to 0 when it has already been done
        [XmlElement("ReminderTimeOption")]
        public ReminderTimeOption ReminderTimeOption { get; set; }

        // who is this reminder for
        [XmlElement("ReminderType")]
        public ReminderType ReminderType { get; set; }

    }
}
