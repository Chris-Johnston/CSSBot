using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CSSBot.Reminders.Models
{
    /// <summary>
    /// Represents a reminder, which will be saved/loaded from a file
    /// Need to use a file so that reminders aren't lost when bot restarts
    /// </summary>
    [XmlRoot("Reminder")]
    public class Reminder
    {
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
        [XmlElement("ReminderTimeOption")]
        public ReminderTimeOption ReminderTimeOption { get; set; }

        // which reminder time options have been elapsed (and messages sent)
        [XmlElement("ReminderTimeStatus")]
        public ReminderTimeOption ReminderTimeStatus { get; set; }

        // who is this reminder for
        [XmlElement("ReminderType")]
        public ReminderType ReminderType { get; set; }

    }
}
