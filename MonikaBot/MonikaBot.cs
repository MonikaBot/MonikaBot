using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using MonikaBot.Commands;
using DSharpPlus.EventArgs;

namespace MonikaBot
{
    public class MonikaBot : IDisposable
    {
        private DiscordClient client;
        private CommandsManager commandManager;
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

            if(config.Token == MonikaBotConfig.BlankTokenString) //this is static so I have to reference by class name vs. an instance of the class.
            {
                Console.WriteLine("Please edit the config file!");

                return;
            }

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
            if (OperatingSystemDetermination.GetUnixName().Contains("Windows 7") || OperatingSystemDetermination.IsOnMac() || OperatingSystemDetermination.IsOnUnix())
            {
                Console.WriteLine("On macOS, Windows 7, or Unix; using WebSocket4Net");
                //only do this on windows 7
                client.SetWebSocketClient<DSharpPlus.Net.WebSocket.WebSocket4NetClient>();
            }
        }

        public void ConnectBot()
        {
            /*
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
            */

            client.Ready += Client_Ready;

            /*
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
            */

            client.GuildAvailable += Client_GuildAvailable;


            /*
            client.MessageCreated += (e) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"@{e.Author.Username} #{e.Channel.Name}:");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($" {e.Message.Content}");

                return Task.Delay(0);
            };
            */
            client.MessageCreated += Client_MessageCreated;

            client.ConnectAsync();
        }

        private Task Client_Ready(DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            Console.WriteLine("Ready!");

            // Print all connected servers.
            string servers = "";
            foreach (DiscordGuild server in e.Client.Guilds.Values)
            {
                servers += server.Name + ", ";
            }

            // Setup the command manager for processing commands.
            commandManager = new CommandsManager(client);
            SetupInternalCommands();

            return Task.Delay(0);
        }

        private Task Client_GuildAvailable(DSharpPlus.EventArgs.GuildCreateEventArgs e)
        {
            Console.WriteLine("Guild available: " + e.Guild.Name);

            //Lists all the channels
            string channels = "Channels: ";
            foreach (var channel in e.Guild.Channels)
            {
                channels += $"{channel.Name} ({channel.Type.ToString()}), ";
            }
            Console.WriteLine(channels);

            //Fancy way to send a message to a channel

            //DiscordChannel channelToSend = e.Guild.Channels.Where(x => x.Name == "dev" && x.Type == ChannelType.Text).First();
            //client.SendMessageAsync(channelToSend, "Can you hear me?");

            return Task.Delay(0);
        }

        private Task Client_MessageCreated(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"@{e.Author.Username} #{e.Channel.Name}:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($" {e.Message.Content}\n");

            if(e.Message.Content.StartsWith(config.Prefix)) // We check if the message received starts with our bot's command prefix. If it does...
            {
                // We move onto processing the command.
                // We pass in the MessageCreateEventArgs so we can get other information like channel, author, etc. The CommandsManager wants these things
                ProcessCommand(e.Message.Content, e);
                      
            }

            return Task.Delay(0);
        }

        /// <summary>
        /// Sets up the internal commands for Monika Bot. This is run only once after she's been connected and provides some internal management and information commands.
        /// </summary>
        private void SetupInternalCommands()
        {
            commandManager.AddCommand(new CommandStub("cmdinfo", "Displays help for a command.", "Help", PermissionType.User, 2, e =>
            {
                if (!String.IsNullOrEmpty(e.Args[0]))
                {
                    ICommand stub = commandManager.Commands.FirstOrDefault(x => x.Key == e.Args[0]).Value;

                    if (stub != null)
                    {
                        string msg = "**Help for " + stub.CommandName + "**";
                        msg += $"\n{stub.Description}";
                        if (!String.IsNullOrEmpty(stub.HelpTag))
                            msg += $"\n\n{stub.HelpTag}";
                        if (stub.Parent != null)
                            msg += $"\nFrom module `{stub.Parent.Name}`";
                        if (stub.ID != null)
                            msg += $"\n`{stub.ID}`";
                        e.Channel.SendMessageAsync(msg);
                    }
                    else
                    {
                        e.Channel.SendMessageAsync("What command?");
                    }
                }
                else
                    e.Channel.SendMessageAsync("What command?");
            }));

            commandManager.AddCommand(new CommandStub("os", "Displays OS info for the bot.", "OS information", PermissionType.User, 0, e =>
            {
                e.Channel.SendMessageAsync($"I'm currently being hosted on a system running `{OperatingSystemDetermination.GetUnixName()}`~!");
                if (OperatingSystemDetermination.IsOnMac())
                {
                    Task.Delay(1000);
                    e.Channel.SendMessageAsync("My favourite!");
                }
            }));

            commandManager.AddCommand(new CommandStub("userinfo", "Displays various user information such as discrim, ID, etc.", "Mostly useful for getting the user ID.", PermissionType.User, 1, e =>
            {
                // Names could be passed in as a mention (Which looks like this: <@05982305980598>) or the actual name.
                string user = e.Args[0];
                DiscordMember userObject = null;
                if (user.StartsWith("<@") && user.EndsWith(">")) //an actual mention so we extract the ID which is after the @
                {
                    string ID = user.Trim('<', '@', '>', '!');
                    if (ID == client.CurrentUser.Id.ToString())
                    {
                        e.Channel.SendMessageAsync("That's me silly!");
                        return;
                    }
                    else
                        userObject = e.Channel.Guild.Members.FirstOrDefault(x => x.Id.ToString() == ID);
                }
                else // a regular username
                {
                    userObject = e.Channel.Guild.Members.FirstOrDefault(x => x.DisplayName.ToLower() == user.ToLower());
                    if (userObject.Id == client.CurrentUser.Id)
                    {
                        e.Channel.SendMessageAsync("That's me silly!");
                        return;
                    }
                }

                if (userObject != null)
                {
                    e.Channel.SendMessageAsync($"User Information for `{userObject.DisplayName}`\n\n```\nUsername: {userObject.Username}" +
                                               $"\nPlaying: {userObject.Presence.Game.Name}\nID: {userObject.Id}\nNickname: {userObject.DisplayName}\n```\n\nI know everything~");

                }
                else
                    e.Channel.SendMessageAsync("Couldn't find the user you requested!");
            }));

            commandManager.AddCommand(new CommandStub("moduleinfo", "Shows information about a specific module.", "", PermissionType.User, 1, cmdArgs =>
            {
                if (cmdArgs.Args.Count > 0 && cmdArgs.Args[0].Length > 0)
                {
                    foreach (var module in commandManager.Modules.ToList())
                    {
                        if (module.Key.Name.ToLower().Trim() == cmdArgs.Args[0].ToLower().Trim())
                        {
                            string msg = $"**About Module {module.Key.Name}**";

                            msg += $"\n{module.Key.Description}\nEnabled: {module.Value}";
                            msg += $"\nCommands: ";
                            module.Key.Commands.ForEach(x => msg += $"{x.CommandName}, ");

                            cmdArgs.Channel.SendMessageAsync(msg);
                            break;
                        }
                    }
                }
            }));

#if DEBUG
            /// This stuff is only loaded if we're working with a "Debug" configuration in Visual Studio
            IModule funModule = new FunModule.FunModule();
            funModule.Install(commandManager);
            Console.WriteLine($"Installed module {funModule.Name} (Desc: {funModule.Description})");
#endif
        }

        private void ProcessCommand(string rawString, MessageCreateEventArgs e)
        {
            // The first thing we do is get rid of the prefix string from the command. We take the message from looking like say this:
            // --say something
            // to
            // say something
            // that way we don't have any excess characters getting in the way.
            string rawCommand = rawString.Substring(config.Prefix.Length);

            // A try catch block for executing the command and catching various things and reporting on errors.
            try
            {
                commandManager.ExecuteOnMessageCommand(rawCommand, e.Channel, e.Author);
            }
            catch (UnauthorizedAccessException ex) // Bad permission
            {
                e.Channel.SendMessageAsync(ex.Message);
            }
            catch (ModuleNotEnabledException x) // Module is disabled
            {
                e.Channel.SendMessageAsync($"{x.Message}");
            }
            catch (Exception ex) // Any other error that could happen inside of the commands.
            {
                e.Channel.SendMessageAsync("Exception occurred while running command:\n```\n" + ex.Message + "\n```");
            }          
        }

        public void Dispose()
        {
            if(client != null)
                client.Dispose();
            config = null;
        }
    }
}
