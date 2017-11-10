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

namespace CSSBot.Reminders
{
    public class ReminderService
    {
        // how long in ms to poll
        private const int PollingRate = 60000;
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

        public void RemoveReminder(ulong guildId, int id)
        {
            var y = m_database.GetCollection<Reminder>("Reminders")
                .Delete(x => x.GuildId == guildId && x.ID == id);
        }

        public Reminder AddReminder(ulong guildId, ulong channelID,
            ulong authorId, string text, DateTime time,
            ReminderType type = ReminderType.Channel)
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
                title = expired.ToString() + " Remains";
            }
            embed.WithTitle(title);
            
            embed.WithCurrentTimestamp();

            // get the author username
            var user = m_client.GetUser(r.AuthorId);
            if (user != null)
                embed.WithFooter("Reminder created by " + user.Username);

            string description;

            //if (option == ReminderTimeOption.OnReminderExpire || option == ReminderTimeOption.ThreeHoursOverdue)
            //{
            //    description = string.Format("Reminder for {0:g}\n**Message**:\n{1}", r.ReminderTime, r.ReminderText, DateTime.Now.Subtract(r.ReminderTime));
            //}
            //else
            //{
            //    description = string.Format("Reminder for {0:g}\n{2} remains.\n**Message**:\n{1}", r.ReminderTime, r.ReminderText, DateTime.Now.Subtract(r.ReminderTime));
            //}

            description = string.Format("Reminder for {0:g} ({2} remains.)\n\n{1}", r.ReminderTime, r.ReminderText, expired);

            string mentionStr = "";

            // include the person we were supposed to ping
            if (r.ReminderType == ReminderType.Author && user != null)
                mentionStr = user.Mention;
            else if (r.ReminderType == ReminderType.Channel)
                mentionStr = "@here";
            else if (r.ReminderType == ReminderType.Guild)
                mentionStr = "@everyone";

            description += "\n\n" + mentionStr;

            embed.WithDescription(description);

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
                    var message = await (m_client.GetGuild(r.GuildId)
                        .GetChannel(r.TextChannelId) as SocketTextChannel)
                        .GetMessageAsync(r.LastReminderMessageId.Value) as SocketMessage;
                    await message.DeleteAsync();
                }

                r.LastReminderMessageId = msg.Id;
            }
            catch(Exception e)
            {

            }
        }
    }
}
