using System;
using System.Collections.Generic;
using System.Text;

namespace CSSBot.Reminders.Models
{
    /// <summary>
    /// Who should be notified when the reminder is finished?
    /// </summary>
    public enum ReminderType : short
    {
        // ping the author when this is done
        Author = 1,
        // @here the channel when this is done
        Channel = 2,
        // @everyone the channel when this is done
        Guild = 3,
        // don't ping anyone
        Default = 4
    }
}
