using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace MonikaBot.Commands
{
    public class ModuleNotEnabledException : Exception
    {
        IModule module;
        public IModule Module
        {
            get { return module; }
        }
        public ModuleNotEnabledException(string message, IModule module) : base(message)
        {
            this.module = module;
        }
    }

    public class BaseModuleToggleException : Exception
    {
        public BaseModuleToggleException(string message) : base(message) { }
    }

    public class CommandsManager
    {
        
        private readonly DiscordClient __client;
        public DiscordClient Client
        {
            get
            {
                return __client;
            }
        }
        

        public Random rng = new Random((int)DateTime.Now.Ticks);

        private Dictionary<string, ICommand> __commands;
        public Dictionary<string, ICommand> Commands
        {
            get { return __commands; }
        }

        /// <summary>
        /// Key value pair of the modules.
        /// Key = module
        /// Value = Whether or not the module is enabled.
        /// </summary>
        private Dictionary<IModule, bool> __modules;
        public Dictionary<IModule, bool> Modules
        {
            get { return __modules; }
        }

        //id, permission 
        private static Dictionary<string, PermissionType> __internalUserRoles;
        public static Dictionary<string, PermissionType> UserRoles
        {
            get
            {
                return __internalUserRoles;
            }
        }

        public void WritePermissionsFile(string path = "permissions.json")
        {
            File.WriteAllText("permissions.json", JsonConvert.SerializeObject(UserRoles));
        }

        public void ReadPermissionsFile(string path = "permissions.json")
        {
            if (File.Exists("permissions.json"))
            {
                var permissionsDictionary = JsonConvert.DeserializeObject<Dictionary<string, PermissionType>>(File.ReadAllText("permissions.json"));
                if (permissionsDictionary == null)
                    permissionsDictionary = new Dictionary<string, PermissionType>();
                OverridePermissionsDictionary(permissionsDictionary);

                Console.WriteLine("Permissions dictionary loaded!");
            }
        }

        internal static PermissionType GetPermissionFromID(string id)
        {
            if (__internalUserRoles.Count > 0)
            {
                foreach(var perm in __internalUserRoles)
                {
                    if (perm.Key == id)
                        return perm.Value;
                }
                return PermissionType.User;
            }
            else
                return PermissionType.User;
        }

        
        public CommandsManager(DiscordClient client)
        {
            __client = client;
            __commands = new Dictionary<string, ICommand>();
            __modules = new Dictionary<IModule, bool>();
            __internalUserRoles = new Dictionary<string, PermissionType>();
            Console.Write("");
        }

        public bool HasPermission(DiscordUser member, PermissionType permission)
        {
            if(__internalUserRoles.ContainsKey(member.Id.ToString()))
            {
                foreach (var perm in __internalUserRoles)
                    if (perm.Key == member.Id.ToString() && (int)perm.Value >= (int)permission)
                        return true;
            }
            return false;
        }

        public void AddPermission(DiscordUser member, PermissionType permission)
        {
            if (__internalUserRoles.ContainsKey(member.Id.ToString()))
                __internalUserRoles.Remove(member.Id.ToString());
            __internalUserRoles.Add(member.Id.ToString(), permission);
        }
        public void AddPermission(string memberID, PermissionType permission)
        {
            if (__internalUserRoles.ContainsKey(memberID))
                __internalUserRoles.Remove(memberID);
            __internalUserRoles.Add(memberID, permission);
        }

        public void OverridePermissionsDictionary(Dictionary<string, PermissionType> dict) => __internalUserRoles = dict;

        public void OverrideModulesDictionary(Dictionary<string, bool> dictionary)
        {
            foreach (IModule kvp in __modules.Keys.ToList())
            {
                if (kvp.Name.ToLower().Trim() != "base")
                {
                    if (dictionary.ContainsKey(kvp.Name.ToLower().Trim()))
                        __modules[kvp] = dictionary[kvp.Name.ToLower().Trim()];
                }
            }
        }

        public Dictionary<string, bool> ModuleDictionaryForJson()
        {
            Dictionary<string, bool> dict = new Dictionary<string, bool>();

            lock(__modules)
            {
                foreach (var kvp in __modules)
                {
                    if (kvp.Key.Name.ToLower().Trim() != "base")
                        dict.Add(kvp.Key.Name, kvp.Value);
                }
            }

            return dict;
        }

        public int ExecuteOnMessageCommand(string rawCommandText, DiscordChannel channel, DiscordUser author)
        {
            string[] split = rawCommandText.Split(new char[] { ' ' }); //splits into args and stuff
#if DEBUG
            Console.Write("[Command Manager] Args: ");
            foreach (var arg in split)
                Console.Write(arg + ", ");
            Console.Write($" (Size: {split.Length}");
            Console.Write("\n");
#endif
            try
            {
                if (!__commands.ContainsKey(split[0]))
                    return 1;
                
                var command = __commands[split[0]];

                if (command != null && command.Parent != null && command.Trigger.HasFlag(CommandTrigger.MessageCreate)) //if it's a generic command without a parent then don't bother doing this.
                {
                    lock(__modules)
                    {
                        if (__modules[command.Parent] == false)
                        {
                            throw new ModuleNotEnabledException($"The specified module {command.Parent.Name} is not enabled.", command.Parent);
                        }
                    }
                }

                if(command != null && command.Trigger.HasFlag(CommandTrigger.MessageCreate))
                {
                    command.Args.Clear();
                    if (command.ArgCount > 0)
                    {
                        string[] argsSplit = rawCommandText.Split(new char[] { ' ' }, command.ArgCount + 1);
                        //adds all the arguments
                        for (int i = 1; i < argsSplit.Length; i++)
                            command.AddArgument(argsSplit[i]);
                    }
                    //finally, executes it
                    command.ExecuteCommand(/*__client,*/ channel, author);
                    return 0;
                }
            }
            catch(UnauthorizedAccessException uaex)
            {
                throw uaex; //no permission
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return 1;
        }

        public int ExecuteOnMentionCommand(string rawCommandText, DiscordChannel channel, DiscordUser author)
        {
            string[] split = rawCommandText.Split(new char[] { ' ' });
            try
            {
                /// The flow of the on mention execution will be something like this
                /// 1. Check and see if there's a command as the second part of the message
                /// 2. if there is, go ahead and figure out what we need to execute
                /// 3. if there isn't, then oh well we'll do some chat stuff instead but that'll be handled (more than likely) inside of Monika bot instead of in a module

                if (split.Length <= 1)
                    return 1;
                if (!__commands.ContainsKey(split[1]))
                    return 1;

                var command = __commands[split[1]];
                if(command != null && command.Parent != null && command.Trigger.HasFlag(CommandTrigger.BotMentioned))
                {
                    lock(__modules)
                    {
                        if(__modules[command.Parent] == false)
                        {
                            throw new ModuleNotEnabledException($"The specified module {command.Parent.Name} is not enabled.", command.Parent);
                        }
                    }
                }

                if (command != null && command.Trigger.HasFlag(CommandTrigger.BotMentioned)) // Verify to make sure the command is only executed if the bot is allowed to execute this command on a Mention trigger
                {
                    command.Args.Clear();
                    if (command.ArgCount > 0)
                    {
                        string[] argsSplit = rawCommandText.Split(new char[] { ' ' }, command.ArgCount + 1);
                        //adds all the arguments
                        for (int i = 1; i < argsSplit.Length; i++)
                            command.AddArgument(argsSplit[i]);
                    }
                    //finally, executes it
                    command.ExecuteCommand(/*__client,*/ channel, author);
                    return 0;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }

            return 1;
        }



        public bool ModuleEnabled(string name)
        {
            lock(__modules)
            {
                foreach (var kvp in __modules)
                {
                    if (kvp.Key.Name.ToLower().Trim() == name.ToLower().Trim())
                    {
                        return __modules[kvp.Key];
                    }
                }
            }
            return false;
        }

        public void EnableModule(string name)
        {
            lock(__modules)
            {
                foreach (var kvp in __modules)
                {
                    if (kvp.Key.Name.ToLower().Trim() == name.ToLower().Trim()) //if module exists
                    {
                        __modules[kvp.Key] = true; //enabled
                        break;
                    }
                }
            }
        }

        public void DisableModule(string name)
        {
            if (name.ToLower().Trim() == "base")
                throw new BaseModuleToggleException("Can't disable base module!");

            lock(__modules)
            {
                foreach (var kvp in __modules)
                {
                    if (kvp.Key.Name.ToLower().Trim() == name.ToLower().Trim()) //if module exists
                    {
                        __modules[kvp.Key] = false; //disable it
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">Message.</param>
        public void SendMessage(string message)
        {
            
        }

        /// <summary>
        /// Adds a generic command without an associated module.
        /// </summary>
        /// <param name="command"></param>
        public void AddCommand(ICommand command) => __commands.Add(command.CommandName, command);

        /// <summary>
        /// Adds a command with an assosciated module.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="fromModule"></param>
        public void AddCommand(ICommand command, IModule fromModule)
        {
            command.Parent = fromModule;
            command.Parent.Commands.Add(command);
            lock(__modules)
            {
                if (!__modules.ContainsKey(fromModule))
                    __modules.Add(fromModule, true);

                if (__modules[fromModule] == false) //if you're adding the command, you're enabling the module.
                    __modules[fromModule] = true;
            }

            __commands.Add(command.CommandName, command);
        }
    }
}
