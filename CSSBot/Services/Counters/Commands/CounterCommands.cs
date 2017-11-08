using Discord.Commands;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot.Counters.Commands
{
    [Group("Counter")]
    [Alias("C")]
    [Summary("Commands that are associated with counting.")]
    public class CounterCommands : ModuleBase
    {
        private readonly CounterService _countService;

        // Dependency injection FTW
        public CounterCommands(CounterService service)
        {
            _countService = service;
        }

        // helper method to ensure that our text is dealt 
        // with consistently
        private string makeRegular(string text) 
            => text.Trim().ToLower();

        /// <summary>
        /// Adds a new counter
        /// </summary>
        /// <param name="counterText"></param>
        /// <returns></returns>
        [Command("MakeCounter")]
        [Alias("NewCounter", "Make", "New")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(Discord.GuildPermission.ManageChannels)]
        public async Task AddCounter([Name("Name")]string counterText)
        {
            // add a new counter (check that it isn't already one that exists)
            // and reply back saying that it has been added

            var matches = _countService.Counters.FindOne(x => x.Text.ToLower().Equals(counterText.ToLower()));

            if(matches == null)
            {
                // didn't match so we can insert
                var n = _countService.MakeNewCounter(counterText.ToLower(), Context.Channel.Id, Context.Guild.Id);
                string reply = string.Format("Ok! I've added a counter for `{0}`.",
                    n.Text);
                await ReplyAsync(reply);
            }
            else
            {
                // reply back if there was a match
                string reply = string.Format("The counter `{0}` already exists, and has a value of: `{1}`",
                    matches.Text, matches.Count);
                await ReplyAsync(reply);
            }
        }

        /// <summary>
        /// Increments an existing counter
        /// </summary>
        /// <param name="counterText"></param>
        /// <returns></returns>
        [Command("Increment")]
        [Alias("+", "Add", "Plus", "Tally", "Tick")]
        [RequireContext(ContextType.Guild)]
        public async Task Increment([Name("Name")]string counterText)
        {
            // increment a counter
            var match = _countService.Counters.FindOne(x => x.Text.ToLower().Equals(counterText.ToLower()));
            if (match != null)
            {
                match.Increment();
                // update our DB
                _countService.Counters.Update(match);

                await ReplyAsync(string.Format("`{0}` : {1}", match.Text, match.Count));
            }
        }

        /// <summary>
        /// Decrements an existing counter
        /// </summary>
        /// <param name="counterText"></param>
        /// <returns></returns>
        [Command("Decrement")]
        [Alias("-", "Subtract", "Minus", "Untick")]
        [RequireContext(ContextType.Guild)]
        public async Task Decrement([Name("Name")]string counterText)
        {
            // decrement a counter
            var match = _countService.Counters.FindOne(x => x.Text.ToLower().Equals(counterText.ToLower()));
            if (match != null)
            {
                match.Decrement();
                // update our DB
                _countService.Counters.Update(match);

                await ReplyAsync(string.Format("`{0}` : {1}", match.Text, match.Count));
            }
        }

        /// <summary>
        /// Removes an existing counter
        /// </summary>
        /// <param name="counterText"></param>
        /// <returns></returns>
        [Command("Delete")]
        [Alias("Remove")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(Discord.GuildPermission.ManageChannels)]
        public async Task Delete([Name("Name")]string counterText)
        { 
            int count = _countService.Counters.Delete(x => x.Text.ToLower().Equals(counterText.ToLower()));
            if (count == 0)
            {
                await ReplyAsync("I couldn't find any matching counters to delete.");
            }
            else
                await ReplyAsync(string.Format("Ok! I've deleted {0} counter(s).", count));
        }

        /// <summary>
        /// Lists all of the counters in the channel
        /// </summary>
        /// <returns></returns>
        [Command("ListChannel")]
        [Alias("List", "Channel")]
        [RequireContext(ContextType.Guild)]
        public async Task ListCounters()
        {
            string returnText = "Counters for this channel:\n";

            var matches = _countService.Counters.Find(x => x.ChannelID == Context.Channel.Id);

            foreach (var c in matches)
            {
                returnText += string.Format("`{0}: {1}`\n", c.Text, c.Count);
            }

            await ReplyAsync(returnText);
        }

        // lists counters in guild
        [Command("ListGuild")]
        [Alias("Guild", "ListServer", "Server")]
        [RequireContext(ContextType.Guild)]
        public async Task ListGuild()
        {
            string returnText = "Counters for this server:\n";

            var matches = _countService.Counters.Find(x => x.GuildID == Context.Guild.Id);

            foreach(var c in matches)
            {
                returnText += string.Format("`{0}: {1}`\n", c.Text, c.Count);
            }

            await ReplyAsync(returnText);
        }

        // sets the counter value
        [Command("Set")]
        [Alias("SetCounter")]
        [RequireContext(ContextType.Guild)]
        public async Task SetCounter([Name("Name")]string text, [Name("Value")]int value)
        {
            // set a counter value
            var match = _countService.Counters.FindOne(x => x.Text.ToLower().Equals(text.ToLower()));
            if (match != null)
            {
                match.SetCount(value);
                // update our DB
                _countService.Counters.Update(match);

                await ReplyAsync(string.Format("`{0}` : {1}", match.Text, match.Count));
            }
        }
    }
}