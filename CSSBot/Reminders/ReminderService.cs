using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

namespace CSSBot.Reminders
{
    public class ReminderService
    {
        // how long in ms to poll
        private const int PollingRate = 1000;

        private ReminderList m_Reminders;
        private Timer m_ReminderTimer;

        private readonly DiscordSocketClient m_client;

        public ReminderService(DiscordSocketClient client)
        {
            m_client = client;

            // load reminders from the file
            LoadReminders();
            
            // add a test reminder
            AddReminder(283013523284557826, 308882801590272003, 163184946742034432, "reminder", DateTime.Now.AddSeconds(1));

            // set up our timer that will poll every minute
            // to see if we need to send out a reminder

            m_ReminderTimer = new Timer(async _ =>
            {
                CheckReminderState();
            }, null, 5000, PollingRate);
        }

        // add a reminder
        public void AddReminder(ulong guildId, ulong channelId, ulong authorId, string text, DateTime time,
            ReminderTimeOption timeOption = ReminderTimeOption.ThirtyMinuteWarning | ReminderTimeOption.OnReminderExpire, 
            ReminderType type = ReminderType.Author)
        {
            if(m_Reminders != null)
            {
                if(m_Reminders.Reminders != null)
                {
                    // the date given has already passed
                    if (DateTime.Now.CompareTo(time) > 0) return;

                    // create a new reminder
                    Reminder n = new Reminder()
                    {
                        GuildId = guildId,
                        TextChannelId = channelId,
                        AuthorId = authorId,
                        ReminderText = text,
                        ReminderTime = time,
                        ReminderTimeOption = timeOption,
                        ReminderType = type
                    };

                    // check reminder time options
                    ReminderTimeOption alreadyPassed = 0;

                    // OnReminderExpire
                    if ((n.ReminderTimeOption & ReminderTimeOption.OnReminderExpire) > 0)
                    {
                        // check that it hasn't passed already
                        if (DateTime.Now.CompareTo(time) > 0)
                        {
                            // it has passed already
                            alreadyPassed |= ReminderTimeOption.OnReminderExpire;
                        }
                    }

                    // 30 min
                    if ((n.ReminderTimeOption & ReminderTimeOption.ThirtyMinuteWarning) > 0)
                    {
                        // check that it hasn't passed already
                        if (DateTime.Now.CompareTo(time.AddMinutes(-30)) > 0)
                        {
                            // it has passed already
                            alreadyPassed |= ReminderTimeOption.ThirtyMinuteWarning;
                        }
                    }

                    // 2 hr
                    if ((n.ReminderTimeOption & ReminderTimeOption.TwoHourWarning) > 0)
                    {
                        // check that it hasn't passed already
                        if (DateTime.Now.CompareTo(time.AddHours(-2)) > 0)
                        {
                            // it has passed already
                            alreadyPassed |= ReminderTimeOption.TwoHourWarning;
                        }
                    }

                    // 6 hr
                    if ((n.ReminderTimeOption & ReminderTimeOption.SixHourWarning) > 0)
                    {
                        // check that it hasn't passed already
                        if (DateTime.Now.CompareTo(time.AddHours(-6)) > 0)
                        {
                            // it has passed already
                            alreadyPassed |= ReminderTimeOption.SixHourWarning;
                        }
                    }

                    if ((n.ReminderTimeOption & ReminderTimeOption.OneDayWarning) > 0)
                    {
                        // if we requested the one day warning
                        // check that it hasn't passed already
                        if (DateTime.Now.CompareTo(time.AddDays(-1)) > 0)
                        {
                            // it has passed already
                            alreadyPassed |= ReminderTimeOption.OneDayWarning;
                        }
                    }

                    if ((n.ReminderTimeOption & ReminderTimeOption.ThreeDayWarning) > 0)
                    {
                        // check that it hasn't passed already
                        if (DateTime.Now.CompareTo(time.AddDays(-3)) > 0)
                        {
                            // it has passed already
                            alreadyPassed |= ReminderTimeOption.ThreeDayWarning;
                        }
                    }

                    if ((n.ReminderTimeOption & ReminderTimeOption.ThreeHoursOverdue) > 0)
                    {
                        // check that it hasn't passed already
                        if (DateTime.Now.CompareTo(time.AddHours(3)) > 0)
                        {
                            // it has passed already
                            alreadyPassed |= ReminderTimeOption.ThreeHoursOverdue;
                        }
                    }

                    n.ReminderTimeStatus = alreadyPassed;

                    // check that the Reminder time isn't in the past
                    // and check that the time options specified haven't passed already

                    m_Reminders.Reminders.Add(n);
                }
            }
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

        private bool CheckIfTimeOptionPassed(DateTime reminderTime, ReminderTimeOption option)
        {
            return DateTime.Now.CompareTo(AdjustDateTimeFromOption(reminderTime, option)) > 0;
        }

        private async void CheckReminderState()
        {
            // debug log when we check states
            await Bot.Log(new Discord.LogMessage(Discord.LogSeverity.Debug, "ReminderService", "Checking reminder state."));

            if(m_Reminders != null)
            {

            }

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
                    m_Reminders = ser.Deserialize(fs) as ReminderList;
                }
            }
            catch(Exception e)
            {
                Bot.Log(
                    new Discord.LogMessage(Discord.LogSeverity.Warning, "ReminderService", "Got exception trying to load.", e));
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
