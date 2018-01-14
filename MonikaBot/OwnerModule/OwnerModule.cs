using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using MonikaBot.Commands;

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
            Name = "base";
            Description = "The base set of modules that cannot be enabled or disabled by the user.";
        }
        public override void Install(CommandsManager manager)
        {
            manager.AddCommand(new CommandStub("selfdestruct", "Shuts the bot down.", "", PermissionType.Owner, cmdArgs =>
            {
                mainEntry.Dispose();
            }), this);
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
                        msg += $"\n`{kvp.Key.Name}` - {kvp.Value.ToString()}";
                    }
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