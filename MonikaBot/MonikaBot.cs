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
        private bool SetupMode = false;
        private string AuthorizationCode;

        public MonikaBot()
        {
            if(!File.Exists("config.json")) //check if the configuration file exists
            {
                new MonikaBotConfig().WriteConfig("config.json");
                Console.WriteLine("Please edit the config file!");

                return;
            }

            /// Load config
            config = new MonikaBotConfig();
            config = config.LoadConfig("config.json");

            /// Verify parts of the config
            if(config.Token == MonikaBotConfig.BlankTokenString) //this is static so I have to reference by class name vs. an instance of the class.
            {
                Console.WriteLine("Please edit the config file!");

                return;
            }
            if(config.OwnerID == "NONE")
            {
                SetupMode = true;
                AuthorizationCode = RandomCodeGenerator.GenerateRandomCode(10);

                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Warning: ");
                Console.ForegroundColor = oldColor;
                Console.Write($"Bot is in setup mode. Please type this command in a channel the bot has access to: \n      {config.Prefix}authenticate {AuthorizationCode}\n");
            }

            /// Setup the Client
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
                //only do this on windows 7 or Unix systems
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
            Console.WriteLine("Connected!!");

            /// Hold off on initing commands and modules until AFTER we've setup with an owner.
            if (SetupMode)
            {
                InitCommands();
            }

            return Task.Delay(0);
        }

        private void InitCommands()
        {
            // Setup the command manager for processing commands.
            commandManager = new CommandsManager(client);
            SetupInternalCommands();


#if DEBUG
            /// This stuff is only loaded if we're working with a "Debug" configuration in Visual Studio
            IModule funModule = new FunModule.FunModule();
            funModule.Install(commandManager);
            Console.WriteLine($"[MODULE]: Installed module {funModule.Name} (Desc: {funModule.Description})");
#endif
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
                if (SetupMode)
                {
                    string[] messageSplit = e.Message.Content.Split(' ');
                    if(messageSplit.Length == 2)
                    {
                        Console.WriteLine($"Args: {messageSplit[0]}, {messageSplit[1]}");
                        if(messageSplit[0] == $"{config.Prefix}authenticate" && messageSplit[1].Trim() == AuthorizationCode)
                        {
                            //we've authenticated!
                            SetupMode = false;
                            AuthorizationCode = "";
                            config.OwnerID = e.Message.Author.Id.ToString();
                            config.WriteConfig();
                            e.Channel.SendMessageAsync($"I'm all yours, {e.Message.Author.Mention}~!");

                            InitCommands();
                        }
                    }

                    return Task.Delay(0);
                }
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
            IModule ownerModule = new OwnerModule.BaseOwnerModules(this);
            ownerModule.Install(commandManager);

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
                            msg += $"\nCommands ({module.Key.Commands.Count} Total): ";

                            foreach(var command in module.Key.Commands)
                            {
                                msg += $"{command.CommandName}, ";
                            }

                            cmdArgs.Channel.SendMessageAsync(msg);
                            break;
                        }
                    }
                }
            }));
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
