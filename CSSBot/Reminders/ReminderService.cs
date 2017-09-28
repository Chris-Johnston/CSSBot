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

namespace CSSBot.Reminders
{
    public class ReminderService
    {
        // how long in ms to poll
        private const int PollingRate = 60000;

        private ReminderList m_Reminders;
        private Timer m_ReminderTimer;

        private readonly DiscordSocketClient m_client;

        //should consider making this a read only list
        public List<Reminder> ActiveReminders
        {
            get
            {
                if(m_Reminders != null && m_Reminders.Reminders != null)
                {
                    return new List<Reminder>(m_Reminders.Reminders);
                }
                // empty list
                return new List<Reminder>();
            }
        }

        public ReminderService(DiscordSocketClient client)
        {
            m_client = client;

            // load reminders from the file
            LoadReminders();

            // add a test reminder
            //AddReminder(283013523284557826, 308882801590272003, 163184946742034432, "reminder", DateTime.Now.AddSeconds(30), ReminderTimeOption.OnReminderExpire | ReminderTimeOption.ThirtyMinuteWarning, ReminderType.Guild);

            // set up our timer that will poll every PollingRate
            // to see if we need to send out a reminder

            //m_ReminderTimer = new Timer(async _ =>
            m_ReminderTimer = new Timer(_ =>
           {
               CheckReminderState();
           }, null, 5000, PollingRate);
        }

        /// <summary>
        /// Removes all reminders that match by guild id and reminder id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public void RemoveReminderById(ulong guildId, int reminderId)
        {
            if(m_Reminders != null && m_Reminders.Reminders != null)
            {
                m_Reminders.Reminders.RemoveAll(x => x.ReminderId == reminderId && x.GuildId == guildId);
            }
        }

        // add a reminder
        public Reminder AddReminder(ulong guildId, ulong channelId, ulong authorId, string text, DateTime time,
            ReminderTimeOption timeOption = ReminderTimeOption.ThirtyMinuteWarning | ReminderTimeOption.OnReminderExpire, 
            ReminderType type = ReminderType.Author)
        {
            if(m_Reminders != null)
            {
                if(m_Reminders.Reminders != null)
                {
                    m_Reminders.ReminderCounter++;

                    // the date given has already passed
                    if (DateTime.Now.CompareTo(time) > 0) return null;

                    // create a new reminder
                    Reminder n = new Reminder()
                    {
                        GuildId = guildId,
                        TextChannelId = channelId,
                        AuthorId = authorId,
                        ReminderText = text,
                        ReminderTime = time,
                        ReminderTimeOption = timeOption,
                        ReminderType = type,
                        ReminderId = m_Reminders.ReminderCounter
                    };

                    // check reminder time options
                    ReminderTimeOption alreadyPassed = n.ReminderTimeOption;

                    // check to see that the options that were asked for haven't ended already
                    foreach (var value in Enum.GetValues(typeof(ReminderTimeOption)))
                    {
                        if( ((n.ReminderTimeOption & (ReminderTimeOption)value ) > 0)
                            && CheckTimeOption(n.ReminderTime, (ReminderTimeOption)value))
                        {
                            //Console.WriteLine("" + value + " " + n.ReminderTimeOption + " " + alreadyPassed);
                            alreadyPassed ^= (ReminderTimeOption)value;
                            //Console.WriteLine("\t" + value + " " + n.ReminderTimeOption + " " + alreadyPassed);
                        }
                    }

                    n.ReminderTimeOption = alreadyPassed;

                    //Console.WriteLine("Adding a new one, but already passed value was " + alreadyPassed);

                    // check that the Reminder time isn't in the past
                    // and check that the time options specified haven't passed already

                    m_Reminders.Reminders.Add(n);

                    SaveReminders();

                    return n;
                }
            }
            return null;
        }

        private DateTime AdjustDateTimeFromOption(DateTime time, ReminderTimeOption option)
        {
            switch(option)
            {
                case ReminderTimeOption.OneDayWarning:
                    return time.AddDays(-1);
                case ReminderTimeOption.OnReminderExpire:
                    return time;
                case ReminderTimeOption.SixHourWarning:
                    return time.AddHours(-6);
                case ReminderTimeOption.ThirtyMinuteWarning:
                    return time.AddMinutes(-30);
                case ReminderTimeOption.ThreeDayWarning:
                    return time.AddDays(-3);
                case ReminderTimeOption.ThreeHoursOverdue:
                    return time.AddHours(3);
                case ReminderTimeOption.TwoHourWarning:
                    return time.AddHours(-2);
            }
            return time;
        }

        /// <summary>
        /// Checks the following conditions
        /// A given time option that was asked for is true
        /// The reminder for this time hasn't been done already
        /// and then check if its passed
        /// </summary>
        /// <param name="r"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        private bool CheckIfReminderNeedsToBeSent(Reminder r, ReminderTimeOption option)
        {
            //return ((r.ReminderTimeOption & option) > 0) // check that this option was asked for
            //    && !((r.ReminderTimeStatus |= option) > 0) // check that it hasn't been done already
            //    && CheckTimeOption(r.ReminderTime, option); // check the time
            //Console.WriteLine("Time option value {0} time status value {1}", r.ReminderTimeOption, "");
            bool a = (r.ReminderTimeOption & option) > 0;
            //bool b = ((r.ReminderTimeStatus ^ option) > 0);
            bool b = true;
            bool c= CheckTimeOption(r.ReminderTime, option);
            return a && b && c;
        }

        private bool CheckTimeOption(DateTime reminderTime, ReminderTimeOption option)
        {
            return DateTime.Now.CompareTo(AdjustDateTimeFromOption(reminderTime, option)) > 0;
        }

        // check the states of all of our reminders
        private async void CheckReminderState()
        {
            // debug log when we check states
            await Bot.Log(new Discord.LogMessage(Discord.LogSeverity.Debug, "ReminderService", "Checking reminder state."));

            if(m_Reminders != null && m_Reminders.Reminders != null && m_Reminders.Reminders.Count != 0)
            {
                // loop through all of our reminders
                foreach(var r in m_Reminders.Reminders)
                {
                    //Console.WriteLine("checking a reminder, options that need to be sent are: " + r.ReminderTimeOption);

                    // iterate through our time options
                    foreach(short value in Enum.GetValues(typeof(ReminderTimeOption)))
                    {
                        // check to see if each of the time options have passed
                        if (CheckIfReminderNeedsToBeSent(r, (ReminderTimeOption)value))
                        {
                            //Console.WriteLine("aaaaaaa");
                            SendReminder(r, (ReminderTimeOption)value);
                        }
                    }
                }

                // remove all that are done
                //m_Reminders.Reminders.RemoveAll(x => x.ReminderTimeOption == x.ReminderTimeStatus);
                m_Reminders.Reminders.RemoveAll(x => x.ReminderTimeOption == 0);
                // save our changes after polling
                SaveReminders();
            }

        }

        // sends a reminder based on the specific time option given
        private async void SendReminder(Reminder r, ReminderTimeOption option)
        {
            // build the embed
            var embed = new EmbedBuilder();

            embed.WithAuthor(m_client.CurrentUser);

            embed.WithTitle(GetReminderTitle(option));
            //Console.WriteLine("sending " + GetReminderTitle(option) + DateTime.Now.ToString());
            embed.WithCurrentTimestamp();

            // get the author username
            var user = m_client.GetUser(r.AuthorId);
            if(user != null)
                embed.WithFooter("Reminder created by " + user.Username);

            string description;

            if (option == ReminderTimeOption.OnReminderExpire || option == ReminderTimeOption.ThreeHoursOverdue)
            {
                description = string.Format("Reminder for {0:g}\n**Message**:\n{1}", r.ReminderTime, r.ReminderText, DateTime.Now.Subtract(r.ReminderTime));
            }
            else
            {
                description = string.Format("Reminder for {0:g}\n{2} remains.\n**Message**:\n{1}", r.ReminderTime, r.ReminderText, DateTime.Now.Subtract(r.ReminderTime));
            }

            // include the person we were supposed to ping
            if (r.ReminderType == ReminderType.Author && user != null)
                description += "\n\n" + user.Mention;
            else if (r.ReminderType == ReminderType.Channel)
                description += "\n\n" + "@here";
            else if (r.ReminderType == ReminderType.Guild)
                description += "\n\n" + "@everyone";

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
            await socketChannel.SendMessageAsync("", false, embed.Build());

            //// and now set that bit true
            //r.ReminderTimeStatus |= option;
            // set that bit false
            r.ReminderTimeOption ^= option;
        }

        private string GetReminderTitle(ReminderTimeOption option)
        {
            switch(option)
            {
                case ReminderTimeOption.OneDayWarning:
                    return "One Day Remaining";
                case ReminderTimeOption.OnReminderExpire:
                    return "Reminder Expired";
                case ReminderTimeOption.SixHourWarning:
                    return "Six Hours Remaining";
                case ReminderTimeOption.ThirtyMinuteWarning:
                    return "Thirty Minutes Remaining";
                case ReminderTimeOption.ThreeDayWarning:
                    return "Three Days Remaining";
                case ReminderTimeOption.ThreeHoursOverdue:
                    return "Three Hours Overdue";
                case ReminderTimeOption.TwoHourWarning:
                    return "Two Hours Remaining";
            }
            return "Reminder";
        }

        /// <summary>
        /// Load the list of reminders from the file
        /// </summary>
        private void LoadReminders()
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(ReminderList));
                using (FileStream fs = new FileStream(Program.GlobalConfiguration.Data.ReminderFilePath, FileMode.Open))
                {
                    //m_Reminders = ser.Deserialize(fs) as ReminderList;
                    var v = ser.Deserialize(fs);
                    m_Reminders = v as ReminderList;
                }
            }
            catch(Exception e)
            {
                Bot.Log(
                    new Discord.LogMessage(Discord.LogSeverity.Warning, "ReminderService", "Got exception trying to load.", e));
            }

            if(m_Reminders == null)
            {
                m_Reminders = new ReminderList();
            }
        }

        private void SaveReminders()
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(ReminderList));
                using (var fs = new FileStream(Program.GlobalConfiguration.Data.ReminderFilePath, FileMode.Create))
                {
                    using (var writer = new StreamWriter(fs))
                    {
                        ser.Serialize(writer, m_Reminders);
                        writer.Flush();
                    }
                }
            }
            catch (Exception e)
            {
                Bot.Log( 
                    new Discord.LogMessage(Discord.LogSeverity.Warning, "ReminderService", "Got exception trying to save.", e));
            }
        }


    }
}
