using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;

namespace MonikaBot.Commands
{
    public class CommandArgs
    {
        public List<string> Args { get; internal set; }
        public DiscordChannel Channel { get; internal set; }
        public DiscordUser Author { get; internal set; }
        public string FromIntegration { get; internal set; }
        public DiscordClient Client { get; internal set; }
        public DiscordMessage Message { get; internal set; }
    }

    [Flags]
    public enum CommandTrigger
    {
        MessageCreate = 0,
        UserJoinedServer,
        BotJoinedServer,
        UserLeftServer,
        BotLeftServer,
        BotMentioned
    }

    public abstract class ICommand
    {
        /// <summary>
        /// The trigger of the command you want to run.
        /// Example: help, random, die, 8ball
        /// </summary>
        public virtual string CommandName { get; set; }

        /// <summary>
        /// A short description of the command.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// A help tag for using the command
        /// </summary>
        public virtual string HelpTag { get; set; }

        /// <summary>
        /// The arguments this command can take.
        /// </summary>
        public virtual List<string> Args { get; set; }

        public virtual int ArgCount { get; set; }

        /// <summary>
        /// The module this command came from.
        /// </summary>
        public virtual IModule Parent { get; internal set; }

        /// <summary>
        /// Sets the event for the command to trigger. 
        /// </summary>
        /// <value>The trigger.</value>
        public virtual CommandTrigger Trigger { get; internal set; } = CommandTrigger.MessageCreate;

        /// <summary>
        /// The permission type that the command takes.
        /// </summary>
        public virtual PermissionType MinimumPermission { get; set; } = PermissionType.User;

        public virtual Action<CommandArgs> Do { get; internal set; }

        internal virtual Type __typeofCommand { get; set; }
        public virtual string ID { get; set; }

        public abstract void ExecuteCommand(DiscordChannel channel, DiscordUser member, DiscordMessage message, DiscordClient client = null);

        //public string ReturnArgument(string argName)
        //{
        //    return Args.Select(m => m).Where(x => x.Key == argName).Select(k => k.Value).First();
        //}

        public void AddArgument(string argValue)
        {
            Args.Add(argValue);
        }
    }
}
