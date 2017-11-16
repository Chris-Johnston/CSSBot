using System;
using System.Collections.Generic;
using System.Text;
using CSSBot.Counters.Models;
using System.Xml.Serialization;
using System.IO;
using LiteDB;

namespace CSSBot.Counters
{
    public class CounterService
    {
        private LiteDatabase _database;
        
        public CounterService(LiteDatabase db)
        {
            _database = db;
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
