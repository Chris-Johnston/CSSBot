using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot
{
    public class NotExpiredPreconditionAttribute : ParameterPreconditionAttribute
    {
        private DateTime now;
        public NotExpiredPreconditionAttribute()
        {
            now = DateTime.Now;
        }
        public NotExpiredPreconditionAttribute(DateTime date)
        {
            now = date;
        }

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
        {
            if (value is DateTime time)
            {
                return now.CompareTo(time) >= 0 ? PreconditionResult.FromSuccess() : PreconditionResult.FromError("The provided time was after the current time.");
            }
            return PreconditionResult.FromError("The type of value was not a DateTime.");
        }
    }
}
