using DSharpPlus;
using System;
using System.IO;

namespace MonikaBot
{
    class MonikaBot : IDisposable
    {
        private DiscordClient client;
        internal MonikaBotConfig config;

        public MonikaBot()
        {
            if(File.Exists("config.json")) //check if the configuration file exists
            {
                config = new MonikaBotConfig();
                config = config.LoadConfig("config.json");
            }
            else
            {
                new MonikaBotConfig().WriteConfig("config.json");
                Console.WriteLine("Please edit the config file!");
            }
        }

        public void Dispose()
        {
            //nothing yet, we have nothing to dispose! 
        }
    }
}
