using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using LiteDB;

namespace CSSBot.Reminders.Models
{
    /// <summary>
    /// Represents a reminder, which will be saved/loaded from a file
    /// Need to use a file so that reminders aren't lost when bot restarts
    /// </summary>
    public class Reminder
    {
        public Reminder()
        {
            ReminderTimeSpanTicks = new List<long>();
            ReminderTime = new DateTime();
        }

        // the unique id of this reminder
        [BsonId]
        public int ID { get; set; }

        // the guild that created this reminder
        [BsonField]
        public ulong GuildId { get; set; }

        // the text channel that this reminder is for
        [BsonField]
        public ulong TextChannelId { get; set; }

        // the author of this reminder
        [BsonField]
        public ulong AuthorId { get; set; }

        // the text that makes up the reminder itself
        [BsonField]
        public string ReminderText { get; set; }

        // when the reminder is set to activate
        [BsonField]
        public DateTime ReminderTime { get; set; }

        /// <summary>
        /// Represents all of the timespans in ticks
        /// </summary>
        [BsonField]
        public List<long> ReminderTimeSpanTicks { get; set; }

        /// <summary>
        /// Represents the ID of the last reminder message that was 
        /// sent, if it exists. This should be deleted before a 
        /// new reminder message is sent
        /// </summary>
        [BsonField]
        public ulong? LastReminderMessageId { get; set; }

        /// <summary>
        /// Represents all of the timespans in TimeSpans
        /// </summary>
        [BsonIgnore]
        public IEnumerable<TimeSpan> ReminderTimeSpans
        {
            get
            {
                foreach (long ticks in ReminderTimeSpanTicks)
                    yield return new TimeSpan(ticks);
            }
        }

        /// <summary>
        /// Used for adding a timespan to the collection of time spans
        /// </summary>
        /// <param name="ts"></param>
        [BsonIgnore]
        public void AddTimeSpan(TimeSpan ts)
        {
            if(!ContainsTimeSpan(ts))
                ReminderTimeSpanTicks.Add(ts.Ticks);
        }

        /// <summary>
        /// Checks our list of timespans and returns the 
        /// most recently expired timespan
        /// Removes any expired timespans
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public TimeSpan? CheckExpiredTimeSpan()
        {
            TimeSpan? mostRecentlyExpired = null;

            // sort our timespans
            // (probably doesn't matter)
            ReminderTimeSpanTicks.Sort();

            List<TimeSpan> toRemove = new List<TimeSpan>();

            // loop through
            foreach(TimeSpan ts in ReminderTimeSpans)
            {
                // work with expired
                if(IsTimeSpanExpired(ts))
                {
                    // find the most expired one
                    if (mostRecentlyExpired.HasValue)
                    {
                        mostRecentlyExpired = GetMoreRecentlyExpired(mostRecentlyExpired.Value, ts);
                    }
                    else
                    {
                        mostRecentlyExpired = ts;
                    }
                }

                // remove expired 
                toRemove.Add(ts);
            }

            // remove our timespans that expired
            foreach(TimeSpan ts in toRemove)
            {
                ReminderTimeSpanTicks.Remove(ts.Ticks);
            }

            return mostRecentlyExpired;
        }

        public TimeSpan GetMoreRecentlyExpired(TimeSpan right, TimeSpan left)
        {
            return (DateTime.Now.Add(right).CompareTo(DateTime.Now.Add(left)) < 0)
                ? right : left;
        }

        public bool IsTimeSpanExpired(TimeSpan ts)
        {
            return DateTime.Now.Add(ts).CompareTo(ReminderTime) > 0;
        }

        [BsonIgnore]
        public bool ContainsTimeSpan(TimeSpan ts)
        {
            return ReminderTimeSpanTicks.Contains(ts.Ticks);
        }

        [BsonIgnore]
        public void RemoveTimeSpan(TimeSpan ts)
        {
            ReminderTimeSpanTicks.Remove(ts.Ticks);
        }
        
        [BsonIgnore]
        public void SetDefaultTimeSpans()
        {
            // 1 Week
            AddTimeSpan(new TimeSpan(7, 0, 0, 0));
            // 3 Days
            AddTimeSpan(new TimeSpan(3, 0, 0, 0));
            // 1 Day
            AddTimeSpan(new TimeSpan(1, 0, 0, 0));
            // 1 hour
            AddTimeSpan(new TimeSpan(1, 0, 0));
            // On Reminder Expire
            AddTimeSpan(new TimeSpan(0));
            // 3 hours overdue
            //AddTimeSpan(new TimeSpan(-3, 0, 0));
        }

        // who should be notified by this reminder, either the channel, the whole guild, or the user who created this
        [XmlElement("ReminderType")]
        public ReminderType ReminderType { get; set; }
    }
}
