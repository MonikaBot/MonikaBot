using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

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

            Console.WriteLine("OS: " + OperatingSystemDetermination.GetUnixName());
            if (OperatingSystemDetermination.GetUnixName().Contains("Windows 7"))
            {
                Console.WriteLine("On Windows 7, using WebSocket4Net");
                //only do this on windows 7
                client.SetWebSocketClient<DSharpPlus.Net.WebSocket.WebSocket4NetClient>();
            }
        }

        public void ConnectBot()
        {
            client.Ready += (e) =>
            {
                Console.WriteLine("Ready!");

                // Print all connected servers.
                string servers = "";
                foreach(DiscordGuild server in e.Client.Guilds.Values)
                {
                    servers += server.Name + ", ";
                }
                Console.WriteLine("Servers: " + servers);

                // Print all connected channels.

                return Task.Delay(0);
            };

            client.GuildAvailable += (e) =>
            {
                Console.WriteLine("Guild available: " + e.Guild.Name);

                //Lists all the channels
                string channels = "Channels: ";
                foreach(var channel in e.Guild.Channels)
                {
                    channels += $"{channel.Name} ({channel.Type.ToString()}), ";
                }
                Console.WriteLine(channels);

                //Fancy way to send a message to a channel

                DiscordChannel channelToSend = e.Guild.Channels.Where(x => x.Name == "dev" && x.Type == ChannelType.Text).First();
                client.SendMessageAsync(channelToSend, "Can you hear me?");

                return Task.Delay(0);
            };

            client.MessageCreated += (e) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"@{e.Author.Username} #{e.Channel.Name}:");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($" {e.Message.Content}");

                return Task.Delay(0);
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
