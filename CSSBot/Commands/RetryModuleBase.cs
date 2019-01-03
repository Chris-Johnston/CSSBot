using CSSBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot.Commands
{
    public class RetryModuleBase : ModuleBase
    {
        public MessageRetryService messageRetry;

        public RetryModuleBase(MessageRetryService messageRetry)
        {
            this.messageRetry = messageRetry;
        }

        /// <summary>
        ///     Replies to the original message, or updates if there was an edit.
        ///     Does not work with commands with multiple message responses.
        /// </summary>
        /// <param name="message"></param>
        internal async Task ReplyOrUpdateAsync(string message = null, bool isTTS = false, Embed embed = null)
        {
            var id = messageRetry.GetMessageToUpdate(Context);
            if (id.HasValue)
            {
                // id already exists
                var m = await Context.Channel.GetMessageAsync(id.Value);
                if (m is IUserMessage usermessage)
                {
                    // update the existing message
                    await usermessage.ModifyAsync(x =>
                    {
                        x.Content = message;
                        x.Embed = embed;
                    });
                    // don't need to update message IDs
                }
            }
            else
            {
                // id doesn't already exist, so create a new message
                var msg = await ReplyAsync(message, isTTS, embed);
                messageRetry.RegisterSuccessfulCommand(Context.Message.Id, msg.Id);
            }
        }
    }
}
