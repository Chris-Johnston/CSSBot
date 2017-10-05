using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CSSBot.Counters.Commands
{

    public class CounterCommands : ModuleBase
    {

        private readonly CounterService _countService;

        // Dependency injection FTW
        public CounterCommands(CounterService service)
        {
            _countService = service;
        }

        /// <summary>
        /// Adds a new counter
        /// </summary>
        /// <param name="counterText"></param>
        /// <returns></returns>
        public async Task AddCounter(string counterText)
        {
            // add a new counter (check that it isn't already one that exists)
            // and reply back saying that it has been added
        }

        public async Task Increment(string counterText)
        {
            // increment a counter
        }

        public async Task Decrement(string counterText)
        {
            // decrement a counter
        }

        public async Task Delete(string counterText)
        {
            // deletes a counter by the text given
        }

        public async Task Delete(int counterID)
        {
            // deletes a counter by the counter id given
        }

        public async Task ListCounters()
        {
            // lists all the counters for the channel
        }
    }
}
