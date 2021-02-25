using System.Collections.Generic;
using System.Linq;

namespace ModLoader.Modding
{
    public class CommandManager : ICommandable
    {
        /// <summary>
        ///     The global instance of CommandManager
        /// </summary>
        public static readonly CommandManager Manager = new CommandManager();

        private readonly List<BaseCommand> _commands = new List<BaseCommand>();

        private CommandManager()
        {
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

        public string CommandHeader => "";
        public bool IsPrimaryCommandContext { get; set; }

        /// <summary>
        ///     Adds your custom command into the game.
        /// </summary>
        /// <param name="command">You command instance</param>
        public void AddCommand(BaseCommand command)
        {
            _commands.Add(command);
        }
    }

    /// <summary>
    ///     An base class that custom mod commands must extend.
    /// </summary>
    public abstract class BaseCommand : CommandDefinition
    {
        protected BaseCommand(string name, string usage) : base(name, usage)
        {
        }

        /// <summary>
        ///     This method is executed when your command is called by the user
        /// </summary>
        /// <param name="command">The context of the current executed command</param>
        /// <param name="partOfMultiCommand">true if the command is part of a multi command execution</param>
        /// <returns>
        ///     true if the command was handled. returning false will continue the search for any other command matching the
        ///     same name
        /// </returns>
        public abstract bool Execute(ExecutedCommand command, bool partOfMultiCommand);
    }
}