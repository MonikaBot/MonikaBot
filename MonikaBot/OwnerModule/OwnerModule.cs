using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using MonikaBot.Commands;
using Newtonsoft.Json;

namespace MonikaBot.OwnerModule
{
    /// <summary>
    /// Defines a set of base owner modules that cannot be disabled.
    /// Commands such as selfdestruct, enablemodule, disablemodule, etc. will be defined here.
    /// </summary>
    public class BaseOwnerModules : IModule
    {
        private MonikaBot mainEntry;

        public BaseOwnerModules(MonikaBot main)
        {
            mainEntry = main;
            Name = "Base";
            Description = "The base set of modules that cannot be enabled or disabled by the user.";
        }
        public override void Install(CommandsManager manager)
        {
            manager.AddCommand(new CommandStub("uptime", "Check how long the bots been running for.", "No arguments", cmdArgs=>
            {
                TimeSpan uptime = DateTime.Now - mainEntry.ReadyTime;
                cmdArgs.Channel.SendMessageAsync($"I've been running for `{uptime.Days} days, {uptime.Hours} hrs, and {uptime.Minutes} mins`~");
            }, trigger: CommandTrigger.BotMentioned | CommandTrigger.MessageCreate), this);
            manager.AddCommand(new CommandStub("selfdestruct", "Shuts the bot down.", "", PermissionType.Owner, cmdArgs =>
            {
                mainEntry.Dispose();
            }), this);
            manager.AddCommand(new CommandStub("reloadmodules", "Reloads the bot's modules", "No Arguments", PermissionType.Admin, cmdArgs=>
            {
                cmdArgs.Channel.SendMessageAsync($"Okay {cmdArgs.Author.Mention}~. Just give me one second!");
                cmdArgs.Channel.TriggerTypingAsync();
                int modulesLoaded = mainEntry.ReloadModules(true);
                Thread.Sleep(2000);
                cmdArgs.Channel.SendMessageAsync($"I'm back! I reloaded {modulesLoaded} module(s) for you!");

            }, trigger: CommandTrigger.BotMentioned | CommandTrigger.MessageCreate), this);
            manager.AddCommand(new CommandStub("removemodules", "Forces all modules out of memory (hopefully).", "No arguments", PermissionType.Admin, cmdArgs=>
            {
                cmdArgs.Channel.SendMessageAsync("Working on it....");
                cmdArgs.Channel.TriggerTypingAsync();
                int modulesLoaded = mainEntry.ReloadModules(false);
                cmdArgs.Channel.SendMessageAsync("Done~");
            }));
            manager.AddCommand(new CommandStub("giveperm", "Gives the perm to the specified user (bot scope)", "", PermissionType.Owner, 2, e =>
            {
                //giveperm Admin <@2309208509852>
                if (e.Args.Count > 1)
                {
                    string permString = e.Args[0];
                    PermissionType type = PermissionType.User;
                    switch (permString.ToLower())
                    {
                        case "admin":
                            type = PermissionType.Admin;
                            break;
                        case "mod":
                            type = PermissionType.Mod;
                            break;
                        case "none":
                            type = PermissionType.None;
                            break;
                        case "user":
                            type = PermissionType.User;
                            break;
                    }
                    string id = e.Args[1].Trim(new char[] { '<', '@', '>' });
                    manager.AddPermission(id, type);
                    e.Channel.SendMessageAsync($"Given permission {type.ToString().Substring(type.ToString().IndexOf('.') + 1)} to <@{id}>!");
                }
                File.WriteAllText("permissions.json", JsonConvert.SerializeObject(CommandsManager.UserRoles));
            }), this);

            manager.AddCommand(new CommandStub("disablemodule", "Disables a module by name", "The module name is case insensitive.", PermissionType.Owner, 1, cmdArgs =>
            {
                if (cmdArgs.Args[0].Length > 0)
                {
                    if (!manager.ModuleEnabled(cmdArgs.Args[0]))
                    {
                        cmdArgs.Channel.SendMessageAsync("Module already disabled!");
                        return;
                    }
                    try
                    {
                        manager.DisableModule(cmdArgs.Args[0]);
                        cmdArgs.Channel.SendMessageAsync($"Disabled {cmdArgs.Args[0]}.");
                    }
                    catch (Exception ex)
                    { cmdArgs.Channel.SendMessageAsync($"Couldn't disable module! {ex.Message}"); }
                }
                else
                {
                    cmdArgs.Channel.SendMessageAsync("What module?");
                }
            }), this);

            manager.AddCommand(new CommandStub("enablemodule", "Disables a module by name", "The module name is case insensitive.", PermissionType.Owner, 1, cmdArgs =>
            {
                if (cmdArgs.Args[0].Length > 0)
                {
                    if (manager.ModuleEnabled(cmdArgs.Args[0]))
                    {
                        cmdArgs.Channel.SendMessageAsync("Module already enabled!");
                        return;
                    }
                    try
                    {
                        manager.EnableModule(cmdArgs.Args[0]);
                        cmdArgs.Channel.SendMessageAsync($"Enabled {cmdArgs.Args[0]}.");
                    }
                    catch (Exception ex)
                    { cmdArgs.Channel.SendMessageAsync($"Couldn't enable module! {ex.Message}"); }
                }
                else
                {
                    cmdArgs.Channel.SendMessageAsync("What module?");
                }
            }), this);

            manager.AddCommand(new CommandStub("modules", "Lists all the modules and whether or not they're enabled.", "",
                PermissionType.Owner, cmdArgs =>
            {
                string msg = $"**Modules**";
                foreach (var kvp in manager.Modules)
                {
                    msg += $"\n`{kvp.Key.Name}` - {(kvp.Value ? "Enabled" : "Disabled")}";
                    if(kvp.Key.ModuleKind == ModuleType.External)
                    {
                        msg += " - From DLL";
                    }
                }
                cmdArgs.Channel.SendMessageAsync(msg);
            }), this);
            manager.AddCommand(new CommandStub("commands", "Lists all of the available commands", "", PermissionType.User, cmdArgs=>
            {
                string msg = "**Commands**\n```";
                foreach(var command in manager.Commands)
                {
                    msg += command.Value.CommandName + ", ";
                }
                msg += "\n```";
                cmdArgs.Channel.SendMessageAsync(msg);
            }), this);
            manager.AddCommand(new CommandStub("changeprefix", "Changes the command prefix to a specified character.", "", PermissionType.Owner, 1, cmdArgs =>
            {
                if (cmdArgs.Args.Count > 0)
                {
                    string oldPrefix = mainEntry.config.Prefix;
                    try
                    {
                        string newPrefix = cmdArgs.Args[0];
                        mainEntry.config.Prefix = newPrefix;
                        cmdArgs.Channel.SendMessageAsync($"Command prefix changed to **{mainEntry.config.Prefix}** successfully!");
                    }
                    catch (Exception)
                    {
                        cmdArgs.Channel.SendMessageAsync($"Unable to change prefix to `{cmdArgs.Args[0]}`. Falling back to `{oldPrefix}`.");
                        mainEntry.config.Prefix = oldPrefix;
                    }
                }
                else
                    cmdArgs.Channel.SendMessageAsync("What prefix?");
            }), this);

            manager.AddCommand(new CommandStub("cmdinfo", "Displays help for a command.", "Help", PermissionType.User, 2, e =>
            {
                if (!String.IsNullOrEmpty(e.Args[0]))
                {
                    ICommand stub = manager.Commands.FirstOrDefault(x => x.Key == e.Args[0]).Value;

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
            }), this);

            manager.AddCommand(new CommandStub("os", "Displays OS info for the bot.", "OS information", PermissionType.User, 0, e =>
            {
                e.Channel.SendMessageAsync($"I'm currently being hosted on a system running `{OperatingSystemDetermination.GetUnixName()}`~!");
                if (OperatingSystemDetermination.IsOnMac())
                {
                    Thread.Sleep(1500);
                    e.Channel.SendMessageAsync("My favourite!");
                }
            }), this);

            manager.AddCommand(new CommandStub("moduleinfo", "Shows information about a specific module.", "", PermissionType.User, 1, cmdArgs =>
            {
                if (cmdArgs.Args.Count > 0 && cmdArgs.Args[0].Length > 0)
                {
                    foreach (var module in manager.Modules.ToList())
                    {
                        if (module.Key.Name.ToLower().Trim() == cmdArgs.Args[0].ToLower().Trim())
                        {
                            string msg = $"**About Module {module.Key.Name}**";

                            msg += $"\n{module.Key.Description}\nEnabled: {module.Value}";
                            msg += $"\nCommands ({module.Key.Commands.Count} Total): ";

                            foreach (var command in module.Key.Commands)
                            {
                                msg += $"{command.CommandName}, ";
                            }

                            cmdArgs.Channel.SendMessageAsync(msg);
                            break;
                        }
                    }
                }
            }), this);

            /* TODO
            manager.AddCommand(new CommandStub("prune",
                "Prunes the specified amount of messages.",
                "If the bot has the role `ManagerMessages`, he will prune other messages in chat.",
                PermissionType.Owner, 1,
            cmdArgs =>
            {
                int messageCount = 0;
                if (int.TryParse(cmdArgs.Args[0], out messageCount))
                {
                    var messagesToPrune = manager.Client.GetMessageHistory(cmdArgs.Channel, messageCount);
                    DiscordMember selfInServer = cmdArgs.Channel.Parent.GetMemberByKey(manager.Client.Me.ID);
                    bool pruneAll = false;
                    if (selfInServer != null)
                    {
                        foreach (var roll in selfInServer.Roles)
                        {
                            if (roll.Permissions.HasPermission(DiscordSpecialPermissions.ManageMessages))
                            {
                                pruneAll = true;
                                break;
                            }
                        }
                        foreach (var roll in cmdArgs.Channel.PermissionOverrides)
                        {
                            if (roll.id == manager.Client.Me.ID)
                            {
                                if (roll.AllowedPermission(DiscordSpecialPermissions.ManageMessages))
                                {
                                    pruneAll = true;
                                    break;
                                }
                            }
                        }
                    }
                    foreach (var msg in messagesToPrune)
                    {
                        if (!pruneAll)
                        {
                            if (msg.Author.ID == manager.Client.Me.ID)
                            {
                                manager.Client.DeleteMessage(msg);
                                Thread.Sleep(100);
                            }
                        }
                        else
                        {
                            manager.Client.DeleteMessage(msg);
                            Thread.Sleep(100);
                        }
                    }
                    cmdArgs.Channel.SendMessage($"Attempted pruning of {messageCount} messages.");
                }
            }));
            */
        }
    }
}