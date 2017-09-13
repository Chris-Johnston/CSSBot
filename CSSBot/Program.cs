using System;

namespace CSSBot
{
    class Program
    {
        // have our GlobalConfiguration class a static property
        // of Program
        public static GlobalConfiguration GlobalConfiguration { get; private set; }

        static void Main(string[] args)
        {
            Console.WriteLine("CSS Bot");

            // set up launch parameters
            // any additional parameters should be added in a similar fashion,
            // however it's probably best to do this via the config file
            // as to remove clutter on the command line

            string configFilePath = null;
            foreach (string arg in args)
            {
                if (arg.StartsWith("-config="))
                {
                    // should probably change this. this works fine for paths that
                    // don't contain any whitespace
                    // but that's not always the case
                    configFilePath = arg.Substring("-config=".Length);
                }
            }

            if (string.IsNullOrWhiteSpace(configFilePath))
            {
                throw new ArgumentException("The config file parameter was not supplied, or was invalid.");
            }

            // initialize the configuration from the path we supplied

            GlobalConfiguration = new GlobalConfiguration(configFilePath);

            new Bot().Start().GetAwaiter().GetResult();
        }
    }
}