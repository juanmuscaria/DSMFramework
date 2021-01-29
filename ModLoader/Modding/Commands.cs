using System.Collections.Generic;
using System.Linq;

namespace ModLoader.Modding
{
    public class CommandManager : ICommandable
    {
        public static readonly CommandManager Manager = new CommandManager();
        private CommandManager()
        {
        }

        private List<BaseCommand> _commands = new List<BaseCommand>();

        public void AddCommand(BaseCommand command)
        {
            _commands.Add(command);
        }

        public List<CommandDefinition> QueryAvailableCommands()
        {
            return _commands.Cast<CommandDefinition>().ToList();
        }

        public List<CommandDefinition> QueryContextCommands()
        {
            return new List<CommandDefinition>();
        }

        public void ExecuteCommand(ExecutedCommand command, bool partOfMultiCommand)
        {
            if (command.Command is BaseCommand)
            {
                var commandToRun = (BaseCommand) command.Command;
                command.Handled = commandToRun.Execute(command, partOfMultiCommand);
            }
        }

        public List<CommandDefinition> QueryDeveloperSpecialCaseCommands()
        {
            return new List<CommandDefinition>();
        }

        public string CommandHeader { get; }
        public bool IsPrimaryCommandContext { get; set; }
    }
    public abstract class BaseCommand : CommandDefinition
    {

        protected BaseCommand(string name, string usage) : base(name, usage)
        {
            
        }
        
        public abstract bool Execute(ExecutedCommand command, bool partOfMultiCommand);
    }
}