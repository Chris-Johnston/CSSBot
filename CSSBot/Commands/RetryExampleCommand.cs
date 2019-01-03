using CSSBot.Services;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot.Commands
{
    public class RetryExampleCommand : RetryModuleBase
    {
        public RetryExampleCommand(MessageRetryService retry) : base(retry)
        { }

        [Command("Test")]
        public async Task TestRetry(int a, int b)
        {
            await ReplyOrUpdateAsync($"{a + b}");
        }
    }
}
