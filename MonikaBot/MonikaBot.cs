using DSharpPlus;
using System;
using System.IO;

namespace MonikaBot
{
    public class MonikaBot : IDisposable
    {
        private DiscordClient client;
        internal MonikaBotConfig config;

        public MonikaBot()
        {
            if(!File.Exists("config.json")) //check if the configuration file exists
            {
                new MonikaBotConfig().WriteConfig("config.json");
                Console.WriteLine("Please edit the config file!");

                return;
            }

            config = new MonikaBotConfig();
            config = config.LoadConfig("config.json");
            DiscordConfiguration dConfig = new DiscordConfiguration
            {
                AutoReconnect = true,
                EnableCompression = true,
                LogLevel = LogLevel.Debug,
                Token = config.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true
            };
            client = new DiscordClient(dConfig);
        }

        public void Dispose()
        {
            //nothing yet, we have nothing to dispose!
        }
    }
}
