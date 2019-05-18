using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CSSBot.Reminders.Models;
using LiteDB;
using Humanizer;

namespace CSSBot.Reminders
{
    public class ReminderService
    {
        // how long in ms to poll
        private const int PollingRate = 30000;
        private Timer m_ReminderTimer;
        private LiteDatabase m_database;
        private readonly DiscordSocketClient m_client;

        public ReminderService(DiscordSocketClient client, LiteDatabase db)
        {
            m_client = client;
            m_database = db;

            m_ReminderTimer = new Timer(_ =>
            {
                // Check Reminders
                CheckReminders();

            }, null, 5000, PollingRate);
        }

        public Reminder GetReminder(ulong guildId, int id)
        {
            return m_database.GetCollection<Reminder>("Reminders").FindOne(
                x => x.GuildId == guildId && x.ID == id);
        }
        public Reminder GetReminder(ulong guildId, int id, ulong authorId)
        {
            return m_database.GetCollection<Reminder>("Reminders").FindOne(
                x => x.GuildId == guildId && x.ID == id && x.AuthorId == authorId);
        }

        public IEnumerable<Reminder> GetReminderChannel(ulong channelID)
        {
            return m_database.GetCollection<Reminder>("Reminders").Find(
                x => x.TextChannelId == channelID);
        }

        public IEnumerable<Reminder> GetReminderGuild(ulong guildId)
        {
            return m_database.GetCollection<Reminder>("Reminders").Find(
                x => x.GuildId == guildId);
        }

        public IEnumerable<Reminder> GetReminderAuthor(ulong auth, ulong guild)
        {
            return m_database.GetCollection<Reminder>("Reminders").Find(
                x => x.AuthorId == auth && x.GuildId == guild);
        }

        private void CheckReminders()
        {
            Bot.Log(new LogMessage(LogSeverity.Debug, "Reminder", "Polling the reminder service."));

            // loop through all of the reminders
            // and get the most recently expired
            // reminder
            // if its not null then actually
            // send the reminder and 
            // delete the old message

            List<Reminder> expired = new List<Reminder>();

            var reminders = m_database.GetCollection<Reminder>("Reminders");
            foreach(var r in reminders.FindAll())
            {
                // get the last expired timespan if it exists
                TimeSpan? ts = r.CheckExpiredTimeSpan();
                if(ts.HasValue)
                {
                    // send a message, ts expired
                    SendReminder(r, ts.Value);

                    // delete the old one
                    // this is done in checkexpiredtimespan
                    if (r.ReminderTimeSpanTicks.Count == 0)
                        expired.Add(r);

                    // set the new mesasge id
                    // this is done in sendreminder

                    // if we are out of reminders
                    // then delete this reminder
                    // actually leave it for now
                }

                // double check for our error case whee we forgot to remove ones 
                // that contained no reminders
                if (r.ReminderTimeSpanTicks.Count == 0)
                    expired.Add(r);

                // update the reminder in case it changes
                UpdateReminder(r);
            }

            foreach( var r in expired)
            {
                m_database.GetCollection<Reminder>("Reminders").Delete(r.ID);
            }
        }
        
        //public ReminderService(DiscordSocketClient client)
        //{
        //    m_client = client;

        //    // load reminders from the file
        //    LoadReminders();

        //    // add a test reminder
        //    //AddReminder(283013523284557826, 308882801590272003, 163184946742034432, "reminder", DateTime.Now.AddSeconds(30), ReminderTimeOption.OnReminderExpire | ReminderTimeOption.ThirtyMinuteWarning, ReminderType.Guild);

        //    // set up our timer that will poll every PollingRate
        //    // to see if we need to send out a reminder

        //    //m_ReminderTimer = new Timer(async _ =>
        //    m_ReminderTimer = new Timer(_ =>
        //   {
        //       CheckReminderState();
        //   }, null, 5000, PollingRate);
        //}

        public void UpdateReminder(Reminder r)
        {
            var coll = m_database.GetCollection<Reminder>("Reminders");
            coll.Update(r);
        }

        public void UpdateReminder(ulong guildId, int id, string text = null, DateTime? time = null, ReminderType? type = null)
        {
            var coll = m_database.GetCollection<Reminder>("Reminders");
            var match = coll.Find(x => x.GuildId == guildId && x.ID == id);
            foreach(Reminder r in match)
            {
                if(!string.IsNullOrEmpty(text))
                    r.ReminderText = text;
                if(time.HasValue)
                    r.ReminderTime = time.Value;
                if (type.HasValue)
                    r.ReminderType = type.Value;

                coll.Update(r);
            }            
        }

        public void UpdateReminderAuthor(ulong guildId, int id, ulong authorID, string text = null, DateTime? time = null, ReminderType? type = null)
        {
            var coll = m_database.GetCollection<Reminder>("Reminders");
            var match = coll.Find(x => x.GuildId == guildId && x.ID == id && x.AuthorId == authorID);
            foreach (Reminder r in match)
            {
                if (!string.IsNullOrEmpty(text))
                    r.ReminderText = text;
                if (time.HasValue)
                    r.ReminderTime = time.Value;
                if (type.HasValue)
                    r.ReminderType = type.Value;

                coll.Update(r);
            }
        }

        public void RemoveReminderTime(ulong guildId, int id, TimeSpan time)
        {
            var col = m_database.GetCollection<Reminder>("Reminders");
            var match = col.Find(x => x.GuildId == guildId && x.ID == id);
            foreach(Reminder r in match)
            {
                r.RemoveTimeSpan(time);
                col.Update(r);
            }
        }

        public void AddReminderTime(ulong guildId, int id, TimeSpan time)
        {
            var col = m_database.GetCollection<Reminder>("Reminders");
            var match = col.Find(x => x.GuildId == guildId && x.ID == id);
            foreach(Reminder r in match)
            {
                r.AddTimeSpan(time);
                col.Update(r);
            }
        }

        public int RemoveReminder(ulong guildId, int id)
        {
            return m_database.GetCollection<Reminder>("Reminders")
                .Delete(x => x.GuildId == guildId && x.ID == id);
        }

        public int RemoveReminderAuthor(ulong guildId, int id, ulong authorId)
        {
            return m_database.GetCollection<Reminder>("Reminders")
                .Delete(x => x.GuildId == guildId && x.ID == id && x.AuthorId == authorId);
        }

        public Reminder AddReminder(ulong guildId, ulong channelID,
            ulong authorId, string text, DateTime time,
            ReminderType type = ReminderType.Default)
        {
            Reminder r = new Reminder()
            {
                GuildId = guildId,
                TextChannelId = channelID,
                AuthorId = authorId,
                ReminderText = text,
                ReminderTime = time,
                ReminderType = type
            };
            r.SetDefaultTimeSpans();
            // remove the expired ones so we aren't spammed when the reminder is made
            r.CheckExpiredTimeSpan();

            m_database.GetCollection<Reminder>("Reminders")
                .Insert(r);

            return r;
        }
        
        private async void SendReminder(Reminder r, TimeSpan expired)
        {
            // build the embed
            var embed = new EmbedBuilder();

            embed.WithAuthor(m_client.CurrentUser);

            string title;
            if(expired.Equals(TimeSpan.Zero))
            {
                title = "Reminder Expired";
            }
            else
            {
                title = expired.Humanize(3, false).Humanize(LetterCasing.Title) + " Remains";
            }
            embed.WithTitle(title);
            
            //embed.WithCurrentTimestamp();

            // get the author username
            var user = m_client.GetUser(r.AuthorId);
            if (user != null)
                embed.WithFooter("Reminder created by " + user.Username);

            string description;

            // when reminder expire, don't include the timespan difference between now and remindertime
            //if (expired.Equals( TimeSpan.Zero))
            //{
            //    description = $"Reminder for {r.ReminderTime.ToString("g")}\n\n{r.ReminderText}";
            //}
            //else
            //{
            //    description = $"Reminder for {r.ReminderTime.ToString("g")} ({(r.ReminderTime - DateTime.Now).Humanize()} remains.)\n\n{r.ReminderText}";
            //}

            description = $"Reminder for {r.ReminderTime.ToString("g")}\n\n{r.ReminderText}";

            if (r.ReminderTimeSpanTicks.Count > 1)
            {
                // list the remaining reminder timespans
                description += "\n\nNext reminder notifications:\n";
                foreach (TimeSpan ts in r.ReminderTimeSpans)
                    description += $"{ts.Humanize(3, false)}\n";
            }

            description = description.TrimEnd();

            string mentionStr = "";

            // get the person that we are supposed to ping
            if (r.ReminderType == ReminderType.Author && user != null)
                mentionStr = user.Mention;
            else if (r.ReminderType == ReminderType.Channel)
                mentionStr = "@here";
            else if (r.ReminderType == ReminderType.Guild)
                mentionStr = "@everyone";

            embed.WithColor(new Color(255, 204, 77));
            embed.WithDescription(description);
            embed.WithFooter("Reminder ID " + r.ID);

            // get the guild
            var guild = m_client.GetGuild(r.GuildId);
            if (guild == null) return;

            // get the channel
            var channel = m_client.GetChannel(r.TextChannelId);
            if (channel == null) return;

            // cast it to a message channel
            IMessageChannel socketChannel = channel as IMessageChannel;
            if (socketChannel == null) return;

            // send the message
            try
            {
                var msg = await socketChannel.SendMessageAsync(mentionStr, false, embed.Build());
                if (r.LastReminderMessageId.HasValue)
                {
                    // get the id of the last reminder message
                    // and delete the message

                    var g = m_client.GetGuild(r.GuildId);
                    var c= g.GetChannel(r.TextChannelId) as ITextChannel;
                    var message = await c.GetMessageAsync(r.LastReminderMessageId.Value) as IMessage;
                    if(message == null)
                    {
                        await Bot.Log(new LogMessage(LogSeverity.Warning, "ReminderService",
                            "Tried to delete an expired reminder notification, but got casting error."));
                    }
                    else
                    {
                        await message.DeleteAsync();
                    } 
                }

                // set the LastReminderMessageId
                r.LastReminderMessageId = msg.Id;

                // actually update this back in the LiteDB
                UpdateReminder(r);
            }
            catch(Exception e)
            {
                // catch permissions issues that may result from sending a message or deleting the old one
                // if we catch, then at lesat update the reminder in the db
                UpdateReminder(r);
            }
        }
    }
}
