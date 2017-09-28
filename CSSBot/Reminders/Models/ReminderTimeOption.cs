using System;
using System.Collections.Generic;
using System.Text;

namespace CSSBot.Reminders
{
    /// <summary>
    // flags for determining how often reminders should be given
    /// </summary>
    [FlagsAttribute]
    public enum ReminderTimeOption : short
    {
        // as soon as the reminder expires
        OnReminderExpire =      0b0000001,
        // 30 minutes beforehand
        ThirtyMinuteWarning =   0b0000010,
        // 2 hours beforehand
        TwoHourWarning =        0b0000100,
        // 6 hours beforehand
        SixHourWarning =        0b0001000,
        // 1 day beforehand
        OneDayWarning =         0b0010000,
        // 3 days beforehand
        ThreeDayWarning =       0b0100000,

        // 3 hours afterwards
        ThreeHoursOverdue =     0b1000000,

        TenMinuteWarning =      0b10000000,

        FiveMinuteWarning =     0b100000000
    }
}
