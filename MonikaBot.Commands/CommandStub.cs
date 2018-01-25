using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;

namespace MonikaBot.Commands
{
    /// <summary>
    /// A basic command providing arguments as strings.
    /// </summary>
    public class CommandStub : ICommand
    {
        internal override Type __typeofCommand
        {
            get
            {
               return typeof(CommandStub);
            }
            set
            {
                base.__typeofCommand = value;
            }
        }

        internal CommandStub()
        {
            this.ID = IDGenerator.GenerateRandomCode();
        }

        /*
        [Obsolete]
        public CommandStub(string name, string description, string helpTag)
        {
            this.ID = IDGenerator.GenerateRandomCode();

            CommandName = name;
            Description = description;
            HelpTag = helpTag;

            Args = new List<string>();
        }

        [Obsolete]
        public CommandStub(string name, string description, CommandTrigger trigger = CommandTrigger.MessageCreate)
        {
            this.ID = IDGenerator.GenerateRandomCode();

            CommandName = name;
            Description = description;
            Trigger = trigger;

            Args = new List<string>();
        }

        [Obsolete]
        public CommandStub(Action<CommandArgs> action)
        {
            this.ID = IDGenerator.GenerateRandomCode();

            Do = action;

            Args = new List<string>();
        }

        [Obsolete]
        public CommandStub(string name, string description, Action<CommandArgs> action, CommandTrigger trigger = CommandTrigger.MessageCreate)
        {
            this.ID = IDGenerator.GenerateRandomCode();

            Do = action;
            CommandName = name;
            Description = description;
            Trigger = trigger;

            Args = new List<string>();
        }

        [Obsolete]
        public CommandStub(string name, string description, string helpTag, Action<CommandArgs> action, CommandTrigger trigger = CommandTrigger.MessageCreate)
        {
            this.ID = IDGenerator.GenerateRandomCode();

            Do = action;
            CommandName = name;
            Description = description;
            HelpTag = helpTag;
            Trigger = trigger;

            Args = new List<string>();
        }

        [Obsolete]
        public CommandStub(string name, string description, string helpTag, PermissionType minPerm, Action<CommandArgs> action, CommandTrigger trigger = CommandTrigger.MessageCreate)
        {
            this.ID = IDGenerator.GenerateRandomCode();

            Do = action;
            CommandName = name;
            Description = description;
            HelpTag = helpTag;
            MinimumPermission = minPerm;
            Trigger = trigger;

            Args = new List<string>();
        }

        [Obsolete]
        public CommandStub(string name, string description, string helpTag, PermissionType minPerm, int argCount, Action<CommandArgs> action, CommandTrigger trigger = CommandTrigger.MessageCreate)
        {
            this.ID = IDGenerator.GenerateRandomCode();

            Do = action;
            CommandName = name;
            Description = description;
            HelpTag = helpTag;
            MinimumPermission = minPerm;
            ArgCount = argCount;
            Trigger = trigger;

            Args = new List<string>();
        }

        [Obsolete]
        public CommandStub(string name, string description, string helpTag, PermissionType minPerm, int argCount, Action<CommandArgs> action)
        {
            this.ID = IDGenerator.GenerateRandomCode();

            Do = action;
            CommandName = name;
            Description = description;
            HelpTag = helpTag;
            MinimumPermission = minPerm;
            ArgCount = argCount;

            Args = new List<string>();
        }
        */

        public CommandStub(string name, string description, string helpTag, Action<CommandArgs> action, PermissionType minPerm = PermissionType.User,
                           int argCount = 0, CommandTrigger trigger = CommandTrigger.MessageCreate)
        {
            ID = IDGenerator.GenerateRandomCode();

            Do = action;
            CommandName = name;
            Description = description;
            HelpTag = helpTag;

            MinimumPermission = minPerm;
            ArgCount = argCount;
            Trigger = trigger;
            Args = new List<string>();
        }

        public override void ExecuteCommand(DiscordChannel channel, DiscordUser member, DiscordMessage message, DiscordClient client = null)
        {
            CommandArgs e = new CommandArgs();
            /*e.FromIntegration = integration.IntegrationName;*/
            e.Args = this.Args;
            e.Author = member;
            e.Channel = channel;
            e.Message = message;
            e.Client = client;

            if ((int)CommandsManager.GetPermissionFromID(member.Id.ToString()) >= (int)MinimumPermission)
                Do.Invoke(e);
            else
                throw new UnauthorizedAccessException($"You have no permission to execute this command! (Minimum needed: {(MinimumPermission.ToString().Substring(MinimumPermission.ToString().IndexOf('.') + 1))})");
        }
    }
}
