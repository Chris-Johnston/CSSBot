using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot.Services
{
    public class MessageRetryService
    {
        public const int MessageLimit = 1000;

        private readonly DiscordSocketClient client;
        /// <summary>
        ///     Stores the resulting message from a single command invocation as the value, keyed by the message ID that originally started it.
        /// </summary>
        private Dictionary<ulong, ulong> CommandMessageResults = new Dictionary<ulong, ulong>();
        /// <summary>
        ///     Stores successful messages, max of LIMIT.
        ///     Should be the same length as CommandMessageResults
        /// </summary>
        private Queue<ulong> SuccessfulMessages = new Queue<ulong>(MessageLimit);

        public ulong? GetMessageToUpdate(ICommandContext context)
        {
            if (CommandMessageResults.ContainsKey(context.Message.Id))
            {
                return CommandMessageResults[context.Message.Id];
            }
            return null;
        }

        /// <summary>
        ///     Command handler that is called when this edited message should be retried.
        /// </summary>
        public event Func<SocketMessage, Task>  OnCommandRetryHandler;

        public MessageRetryService(DiscordSocketClient client)
        {
            this.client = client;
            SetupHandlers();
        }

        public void RegisterSuccessfulCommand(ulong messageId, ulong newMessage)
        {
            Console.WriteLine($"Registering {messageId} -> {newMessage}");
            // only register if not already contained in the queue
            if (!CommandMessageResults.ContainsKey(messageId))
            {
                // regiser that this command was successful
                SuccessfulMessages.Enqueue(messageId);
                CommandMessageResults.Add(messageId, newMessage);

                // stop tracking old messages
                while (SuccessfulMessages.Count > MessageLimit)
                {
                    var removed = SuccessfulMessages.Dequeue();
                    Console.WriteLine($"removed old id {removed}");
                    if (CommandMessageResults.ContainsKey(removed))
                    {
                        CommandMessageResults.Remove(removed);
                    }
                }
            }
        }

        private void SetupHandlers()
        {
            client.MessageUpdated += Client_MessageUpdated;
            client.MessageDeleted += Client_MessageDeleted;
        }

        private async Task Client_MessageDeleted(Discord.Cacheable<Discord.IMessage, ulong> arg1, ISocketMessageChannel arg2)
        {
            // if the original message was successful and is deleted, delete the related messages
            if (CommandMessageResults.ContainsKey(arg1.Id))
            {
                var messageId = CommandMessageResults[arg1.Id];
                
                try
                {
                    // delete the result of the original command message
                    await arg2.DeleteMessageAsync(messageId);
                }
                catch (HttpException)
                { }
            }
        }

        private async Task Client_MessageUpdated(Discord.Cacheable<Discord.IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
        {
            if (CommandMessageResults.ContainsKey(after.Id))
            {
                // todo consider making the limit on update time-limited as well
                // re-invoke the command handler. this assumes that the command module is using ReplyOrUpdateAsync
                await OnCommandRetryHandler?.Invoke(after);
            }
        }
    }
}
