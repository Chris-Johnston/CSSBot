using System;
using Xunit;
using CSSBot.Reminders.Models;

namespace CSSBot.Tests
{
    public class ReminderTests
    {
        [Fact]
        public void TestReminder()
        {
            // some basic tests of Reminder
            Reminder r = new Reminder();

            Assert.Empty(r.ReminderTimeSpans);
            Assert.Empty(r.ReminderTimeSpanTicks);

            TimeSpan t1 = new TimeSpan(0, 0, -5);
            TimeSpan t2 = new TimeSpan(0, 0, -10);

            r.AddTimeSpan(t1);
            r.AddTimeSpan(t1);

            Assert.True(r.ContainsTimeSpan(t1));
            Assert.False(r.ContainsTimeSpan(t2));
            Assert.Equal(1, r.ReminderTimeSpanTicks.Count);

            r.RemoveTimeSpan(t1);
            Assert.False(r.ContainsTimeSpan(t1));

            r.AddTimeSpan(t1);
            r.AddTimeSpan(t2);

            Assert.Equal(2, r.ReminderTimeSpanTicks.Count);

            r.SetDefaultTimeSpans();

            Assert.NotEqual(2, r.ReminderTimeSpanTicks.Count);

            Assert.NotNull(r.ReminderTime);

            Assert.Equal(t2, r.GetMoreRecentlyExpired(t1, t2));
            Assert.True(r.IsTimeSpanExpired(t1));
            Assert.True(r.IsTimeSpanExpired(new TimeSpan(0)));
        }
    }
}
