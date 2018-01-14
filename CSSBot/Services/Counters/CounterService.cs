using System;
using System.Collections.Generic;
using System.Text;
using CSSBot.Counters.Models;
using System.Xml.Serialization;
using System.IO;
using LiteDB;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace CSSBot.Counters
{
    public class CounterService
    {
        private LiteDatabase _database;
        private DiscordSocketClient _client;
        
        public CounterService(LiteDatabase db, DiscordSocketClient client)
        {
            _database = db;
            _client = client;

            _client.MessageReceived += _client_MessageReceived;
        }

        private Task _client_MessageReceived(SocketMessage arg)
        {
            if (arg.Channel == null) return Task.CompletedTask;

            // when a message received
            // get all of the counters for that channel
            foreach(var counter in Counters.Find(x => x.ChannelID == arg.Channel.Id))
            {
                // if there is a match, increment the counter
                if(arg.Content.ToLower().Contains(counter.Text))
                {
                    counter.Increment();
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// The list of all the counters
        /// </summary>
        public LiteCollection<Counter> Counters
        {
            get
            {
                return _database.GetCollection<Counter>("counters");
            }
        }

        public void UpdateCounter(Counter c)
        {
            _database.GetCollection<Counter>("Counters").Update(c);
        }

        public Counter GetCounterByText(string txt, ulong guildId, ulong channelID)
        {
            txt = txt.ToLower().Trim();
            return Counters
                .FindOne(x => x.Text.Equals(txt) && x.GuildID == guildId && x.ChannelID == channelID);
        }

        /// <summary>
        /// The method that should be used for generating new counters
        /// </summary>
        /// <param name="counterText"></param>
        /// <returns></returns>
        public Counter MakeNewCounter(string counterText, ulong channelID, ulong guildID)
        {
            // do NOT set the ID, we are letting LiteDB do that for us (yay)
            Counter newCounter = new Counter()
            {
                ChannelID = channelID,
                GuildID = guildID,
                Text = counterText,
                Count = 0
            };

            Counters.Insert(newCounter);

            // return the value that we inserted
            //return id;
            return newCounter;
        }
    }
}
