using System;
using System.Collections.Generic;
using System.Text;
using CSSBot.Counters.Models;
using System.Xml.Serialization;
using System.IO;

namespace CSSBot.Counters
{
    public class CounterService
    {
        private CounterFile m_counterData;
        private string m_Path;

        public CounterService()
        {
            m_Path = Program.GlobalConfiguration.Data.CounterFilePath;
            LoadCounterFile(m_Path);
        }

        /// <summary>
        /// Saves all changes made to the counters
        /// </summary>
        public void Save()
        {
            SaveCounterFile(m_Path);
        }

        /// <summary>
        /// The list of all the counters
        /// </summary>
        public List<Counter> Counters
        {
            get
            {
                if (m_counterData != null && m_counterData.Counters != null)
                    return m_counterData.Counters;
                return new List<Counter>();
            }
        }

        /// <summary>
        /// The method that should be used for generating new counters
        /// </summary>
        /// <param name="counterText"></param>
        /// <returns></returns>
        public Counter MakeNewCounter(string counterText, int channelID)
        {
            if (m_counterData != null && m_counterData.Counters != null)
            {
                Counter newCounter = new Counter()
                {
                    ChannelID = channelID,
                    Text = counterText,
                    ID = m_counterData.CounterCount + 1,
                    Count = 0
                };

                return newCounter;
            }
            return null;
        }

        /// <summary>
        /// Loads the counter file
        /// </summary>
        /// <param name="path"></param>
        private void LoadCounterFile(string path)
        {
            // try loading the counter file
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(CounterFile);
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    var v = ser.Deserialize(fs);
                    m_counterData = v as CounterFile;
                }
            }
            catch (Exception e)
            {
                Bot.Log(
                    new Discord.LogMessage(Discord.LogSeverity.Warning, "CounterService", "Tried to load counter data and got exception.", e));
            }

            // if we get here without populating, then something went wrong
            // lets just create it here for the first time
            if(m_counterData == null)
            {
                m_counterData = new CounterFile();
                // and then save it
                SaveCounterFile(path);
            }
        }

        /// <summary>
        /// Saves the counter file
        /// </summary>
        /// <param name="path"></param>
        private void SaveCounterFile(string path)
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(CounterFile));
                using (var fs = new FileStream(path, FileMode.Create))
                {
                    using (var writer = new StreamWriter(fs))
                    {
                        ser.Serialize(writer, m_counterData);
                        writer.Flush();
                    }
                }
            }
            catch(Exception e)
            {
                Bot.Log(new Discord.LogMessage(Discord.LogSeverity.Warning, "CounterService", "Error trying to save file!", e));
            }
        }

    }
}
