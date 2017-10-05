using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CSSBot.Counters.Models
{
    // stores the information that is kept in the counter file
    [XmlRoot("CounterDefinitions")]
    public class CounterFile
    {
        public int CounterCount
        {
            get
            {
                if (Counters == null) return 0;
                return Counters.Count;
            }
        }

        // the list of all the counters
        [XmlElement("Counters")]
        public List<Counter> Counters { get; set; }

        public CounterFile()
        {
            Counters = new List<Counter>();
        }
    }
}
