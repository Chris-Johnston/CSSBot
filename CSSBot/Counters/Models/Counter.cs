using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CSSBot.Counters.Models
{
    public class Counter
    {
        // the ID associated with this counter
        [XmlElement("ID")]
        public int ID { get; set; }

        [XmlElement("ChannelID")]
        public ulong ChannelID { get; set; }

        // which ever text is associated with this counter
        [XmlElement("Text")]
        public string Text { get; set; }

        // the count of this counter
        [XmlElement("Count")]
        public int Count { get; set; }

        /// <summary>
        /// Constructor. Sets count to 0
        /// </summary>
        public Counter()
        {
            Count = 0;
        }
    }
}
