using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using LiteDB;

namespace CSSBot.Counters.Models
{
    public class Counter
    {
        // the unique ID associated with this counter
        [BsonId]
        public int ID { get; set; }

        [BsonField]
        public ulong ChannelID { get; set; }

        [BsonField]
        public ulong GuildID { get; set; }

        // which ever text is associated with this counter
        [BsonField]
        public string Text { get; set; }

        // the count value of this counter
        [BsonField]
        public int Count { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Counter()
        {
            // wait on this, just in case litedb calls this and the value of the 
            // counter gets reset to zero each time, which would be bad
            //Count = 0;
        }

        public int ResetCount() { return Count = 0; }

        public int SetCount(int c) { return Count = c; }

        public int Increment() { return Count++; }

        public int Decrement() { return Count--; }
    }
}
