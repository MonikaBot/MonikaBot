using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using MonikaBot.Commands;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;

namespace MonikaBot
{

    public class MonikaBot : IDisposable
    {
        private DiscordClient client;
        private CommandsManager commandManager;
        internal MonikaBotConfig config;
        private bool SetupMode = false;
        private string AuthorizationCode;
        internal DateTime ReadyTime;

        public MonikaBot()
        {
            if (!File.Exists("config.json")) //check if the configuration file exists
            {
                new MonikaBotConfig().WriteConfig("config.json");
                Console.WriteLine("Please edit the config file!");

                return;
            }

            /// Load config
            config = new MonikaBotConfig();
            config = config.LoadConfig("config.json");

            /// Verify parts of the config
            if (config.Token == MonikaBotConfig.BlankTokenString) //this is static so I have to reference by class name vs. an instance of the class.
            {
                Console.WriteLine("Please edit the config file!");

                return;
            }
            if (config.OwnerID == MonikaBotConfig.BlankOwnerIDString || String.IsNullOrEmpty(config.OwnerID))
            {
                SetupMode = true;
                AuthorizationCode = RandomCodeGenerator.GenerateRandomCode(10);

                Log(LogLevel.Warning, $"Bot is in setup mode. Please type this command in a channel the bot has access to: \n      {config.Prefix}authenticate {AuthorizationCode}\n");
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

            Log(LogLevel.Info, "OS: " + OperatingSystemDetermination.GetUnixName());
            if (OperatingSystemDetermination.GetUnixName().Contains("Windows 7") || OperatingSystemDetermination.IsOnMac() || OperatingSystemDetermination.IsOnUnix())
            {
                Log(LogLevel.Info, "On macOS, Windows 7, or Unix; using WebSocket4Net");
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
            ReadyTime = DateTime.Now;

            commandManager = new CommandsManager(client);
            /// Hold off on initing commands and modules until AFTER we've setup with an owner.
            if (!SetupMode)
            {
                commandManager.ReadPermissionsFile();
                if (CommandsManager.UserRoles.Count == 0 && !String.IsNullOrEmpty(config.OwnerID))
                    CommandsManager.UserRoles.Add(config.OwnerID, PermissionType.Owner);


                InitCommands();
                LoadModules();
            }

            return Task.Delay(0);
        }

        private void InitCommands()
        {
            // Setup the command manager for processing commands.
            SetupInternalCommands();
#if DEBUG && ASDLASDFASDFASDFASDFASDFASDFASDFASDF
            /// This stuff is only loaded if we're working with a "Debug" configuration in Visual Studio
            IModule funModule = new FunModule.FunModule();
            funModule.Install(commandManager);
            Console.WriteLine($"[MODULE]: Installed module {funModule.Name} (Desc: {funModule.Description})");
#endif
        }

        internal int LoadModules()
        {
            int modulesLoaded = 0;
            IEnumerable dllEnumerable = Directory.EnumerateFiles("modules", "*.dll");
#if DEBUG
            string dllsString = "";
            foreach (var filePath in Directory.EnumerateFiles("modules", "*.dll"))
            {
                dllsString += $"{filePath} (Valid: {IsValidModule(filePath)}), ";

            }
            Log(LogLevel.Debug, $"DLLs in modules directory: {dllsString}");
#endif
            foreach (var module in dllEnumerable)
            {
                if (IsValidModule(module.ToString()))
                {
                    IModule moduleToInstall = GetModule(module.ToString());
                    if (moduleToInstall != null)
                    {
                        Log(LogLevel.Debug, $"Installing module {moduleToInstall.Name} from DLL");
                        moduleToInstall.Install(commandManager);
                        modulesLoaded++;
                    }
                }
            }
            return modulesLoaded;
        }

        private bool IsValidModule(string modulePath)
        {
            if (File.Exists(modulePath))
            {
#if DEBUG

                Console.WriteLine($"Verifying module at {modulePath}");
#endif
                Assembly module = Assembly.LoadFrom(modulePath);
                AssemblyName[] references = module.GetReferencedAssemblies();

#if lol

                foreach(var refAssemblyName in references)
                {
                    Log(LogLevel.Debug, "     Depends on: " + refAssemblyName.Name);
                    if (Constants.DontLoadTheseDependencies.Contains(refAssemblyName.Name))
                    { continue; }

                    if (refAssemblyName.Name == "MonikaBot.Commands")
                        Assembly.LoadFrom("../MonikaBot.CommandManager.dll");
                    else
                        Assembly.LoadFrom(refAssemblyName.Name);
                }
#endif
                try
                {
                    Type type = module.GetType("ModuleEntryPoint");
                    if (type != null)
                    {
                        object o = Activator.CreateInstance(type);
                        if (o != null)
                        {
                            return true;
                        }
                    }
                }
                catch(Exception ex)
                {
                    Log(LogLevel.Critical, $"Error when Loading Module: {ex.Message}");
                }

                module = null;
            }

            return false;
        }

        private IModule GetModule(string modulePath)
        {
#if DEBUG
            Log(LogLevel.Debug, $"Loading module at {modulePath}");
#endif
            Assembly module = Assembly.LoadFrom(modulePath);
            Type type = module.GetType("ModuleEntryPoint");
            object o = Activator.CreateInstance(type);
            if(o != null)
            {
                IModule moduleCode = (o as IModuleEntryPoint).GetModule();
                if (moduleCode.ModuleKind != ModuleType.External)
                    return null;
                
                Log(LogLevel.Info, $"Module loaded successfully! {moduleCode.Name}: {moduleCode.Description}");
                return moduleCode;
            }
            return null;
        }

        private Task Client_GuildAvailable(DSharpPlus.EventArgs.GuildCreateEventArgs e)
        {
            
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

            if(e.Message.MentionedUsers.Contains(client.CurrentUser))
            {
                if(!SetupMode)
                {
                    ProcessCommand(e.Message.Content, e, CommandTrigger.BotMentioned);
                }
            }
            else if(e.Message.Content.StartsWith(config.Prefix)) // We check if the message received starts with our bot's command prefix. If it does...
            {
                if (SetupMode)
                {
                    string[] messageSplit = e.Message.Content.Split(' ');
                    if(messageSplit.Length == 2)
                    {
                        if(messageSplit[0] == $"{config.Prefix}authenticate" && messageSplit[1].Trim() == AuthorizationCode)
                        {
                            //we've authenticated!
                            SetupMode = false;
                            AuthorizationCode = "";
                            config.OwnerID = e.Message.Author.Id.ToString();
                            config.WriteConfig();
                            e.Channel.SendMessageAsync($"I'm all yours, {e.Message.Author.Mention}~!");
                            commandManager.AddPermission(e.Message.Author, PermissionType.Owner);
                            commandManager.WritePermissionsFile(); //Write the permissions file to the disk so she remembers who her owner is.

                            InitCommands();
                        }
                    }

                    return Task.Delay(0);
                }
                // We move onto processing the command.
                // We pass in the MessageCreateEventArgs so we can get other information like channel, author, etc. The CommandsManager wants these things
                ProcessCommand(e.Message.Content, e, CommandTrigger.MessageCreate);
                      
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
        }

        private void ProcessCommand(string rawString, MessageCreateEventArgs e, CommandTrigger type)
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
                if (type == CommandTrigger.MessageCreate)
                    commandManager.ExecuteOnMessageCommand(rawCommand, e.Channel, e.Author);
                else if (type == CommandTrigger.BotMentioned)
                    commandManager.ExecuteOnMentionCommand(rawCommand, e.Channel, e.Author);
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
            File.WriteAllText("settings.json", JsonConvert.SerializeObject(config));
            if(CommandsManager.UserRoles != null && CommandsManager.UserRoles.Count > 0)
                File.WriteAllText("permissions.json", JsonConvert.SerializeObject(CommandsManager.UserRoles));

            if(client != null)
                client.Dispose();
            config = null;
        }

        public static void Log(LogLevel level, string text)
        {
            ConsoleColor oldColor = Console.ForegroundColor;

            switch(level)
            {
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Critical:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    break;
            }

            Console.Write($"[{DateTime.Now.ToString()}] [MonikaBot] [{level.ToString()}] ");
            Console.ForegroundColor = oldColor;
            Console.Write($"{text}\n");
        }
    }
}
