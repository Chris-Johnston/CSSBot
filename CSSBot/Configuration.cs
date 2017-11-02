using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CSSBot
{
    /// <summary>
    /// This class is used to store configuration data
    /// That can be loaded (or reloaded, if needed)
    /// from and XML file
    /// </summary>
    [XmlRoot("Configuration")]
    public class Configuration
    {
        [XmlElementAttribute("ConnectionToken")]
        public string ConnectionToken { get; set; }
        
        [XmlElement("LiteDatabasePath")]
        public string LiteDatabasePath { get; set; }

        // ReminderFilePath and CounterFilePath need to be deleted once we have LiteDB fully implemented

        // the path to the reminder file
        [XmlElement("ReminderFilePath")]
        public string ReminderFilePath { get; set; }

        // the path to the counter file
        [XmlElement("CounterFilePath")]
        public string CounterFilePath { get; set; }
    }
}
