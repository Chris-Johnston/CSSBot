using Discord.Commands;
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
        public async Task AddCounter(string counterText)
        {
            // add a new counter (check that it isn't already one that exists)
            // and reply back saying that it has been added

            // try to find a counter that matches
            var counter = _countService.Counters.Find(
                x => x.Text.Equals(makeRegular(counterText)) && x.ChannelID == Context.Channel.Id);
            if(counter == null)
            {
                counter = _countService.MakeNewCounter(makeRegular(counterText), Context.Channel.Id, Context.Guild.Id);
                _countService.Save();
                await ReplyAsync(string.Format("Ok, I've made a new counter for `{0}`.", counter.Text));
            }
            else
            {
                await ReplyAsync(string.Format("This counter (`{0}`) already exists, and has a value of `{1}`.", 
                    counter.Text, counter.Count));
            }
        }

        [Command("Increment")]
        [Alias("+", "Add", "Plus", "Tally", "Tick")]
        [RequireContext(ContextType.Guild)]
        public async Task Increment(string counterText)
        {
            // increment a counter
            var counter = _countService.Counters.Find(
                x => x.Text.Equals(makeRegular(counterText)) && x.ChannelID == Context.Channel.Id);
            if (counter != null)
            {
                counter.Count++;
                _countService.Save();
                await ReplyAsync(string.Format("`{0}` : {1}", counter.Text, counter.Count));
            }
        }

        [Command("Decrement")]
        [Alias("-", "Subtract", "Minus", "Untick")]
        [RequireContext(ContextType.Guild)]
        public async Task Decrement(string counterText)
        {
            // increment a counter
            var counter = _countService.Counters.Find(
                x => x.Text.Equals(makeRegular(counterText)) && x.ChannelID == Context.Channel.Id);
            if (counter != null)
            {
                counter.Count--;
                _countService.Save();
                await ReplyAsync(string.Format("`{0}` : {1}", counter.Text, counter.Count));
            }
        }

        [Command("Delete")]
        [Alias("Remove")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(Discord.GuildPermission.ManageChannels)]
        public async Task Delete(string counterText)
        {
            // deletes a counter by the text given
            var counter = _countService.Counters.Find(
                x => x.Text.Equals(makeRegular(counterText)) && x.ChannelID == Context.Channel.Id);
            if (counter != null)
            {
                _countService.Counters.RemoveAll(x => x.ID == counter.ID);
                _countService.Save();
                await ReplyAsync(string.Format("Deleted `{0}` : {1}", counter.Text, counter.Count));
            }
        }

        [Command("ListChannel")]
        [RequireContext(ContextType.Guild)]
        public async Task ListCounters()
        {
            // lists all the counters for the channel
            string returnText = string.Format("Counters for this channel:\n");
            _countService.Counters.FindAll(
                x => x.ChannelID == Context.Channel.Id && x.GuildID == Context.Guild.Id)
                .ForEach(
                x => returnText += string.Format("`{0}: {1}`\n", x.Text, x.Count));
            await ReplyAsync(returnText);
        }

        [Command("ListGuild")]
        public async Task ListGuild()
        {
            string returnText = string.Format("Counters for this guild:\n");
            _countService.Counters.FindAll
                (
                x => x.GuildID == Context.Guild.Id
                )
                .ForEach
                (
                x => returnText = string.Format("`{0}: {1}`\n", x.Text, x.Count));

            await ReplyAsync(returnText);
        }
    }
}
