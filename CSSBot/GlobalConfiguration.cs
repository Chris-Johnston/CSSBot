using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace CSSBot
{
    /// <summary>
    /// This class should be used for containing Configuration classes
    /// or classes that depend on the configuration and that should 
    /// be re-used
    /// Also used for storing constants (CommandPrefix)
    /// </summary>
    public class GlobalConfiguration
    {
        // we shouldn't need to be changing this often, if at all
        public const char CommandPrefix = '?';

        // the path to the configuration file
        private string m_ConfigFilePath = null;

        // our configuration data, loaded from xml file
        private Configuration m_Data;
        public Configuration Data
        {
            get { return m_Data; }
        }

        /// <summary>
        /// Constructor for GlobalConfiguration class
        /// </summary>
        /// <param name="path"></param>
        public GlobalConfiguration(string path)
        {
            // set the path
            SetConfigurationFilePath(path);
            // load from it
            LoadConfiguration();
        }

        /// <summary>
        /// Sets the Configuration File Path
        /// </summary>
        /// <param name="path"></param>
        private void SetConfigurationFilePath(string path)
        {
            if(string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(
                    paramName: nameof(path),
                    message: "The path supplied was null or whitespace.");
            }

            m_ConfigFilePath = path;
        }

        /// <summary>
        /// Unused constructor
        /// </summary>
        private GlobalConfiguration() { }

        /// <summary>
        /// Loads (or Reloads) the Configuration file
        /// </summary>
        /// <param name="path"></param>
        public void LoadConfiguration()
        {
            if (string.IsNullOrWhiteSpace(m_ConfigFilePath))
            {
                throw new Exception("The configuration file path has not been set.");
            }

            XmlSerializer ser = new XmlSerializer(typeof(Configuration));

            using (FileStream fs = new FileStream(m_ConfigFilePath, FileMode.Open))
            {
                m_Data = (Configuration)ser.Deserialize(fs);
            }
        }
    }
}
