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

        public void ConnectBot()
        {
            client.Ready += (e) =>
            {
                Console.WriteLine("Ready!");

                // Print all connected servers.
                string servers = "";
                foreach(var server in e.Client.Guilds.Keys)
                {
                    servers += server.ToString();
                }

                // Print all connected channels.

                return null;
            };

            client.ConnectAsync();
        }

        public void Dispose()
        {
            if(client != null)
                client.Dispose();
            config = null;
        }
    }
}
