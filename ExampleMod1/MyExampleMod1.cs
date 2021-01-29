using ModLoader.Modding;

namespace ExampleMod1
{
    public class MyExampleMod1 : IMod
    {
        public void Load()
        {
            CommandManager.Manager.AddCommand(new ExampleCommand());
        }
    }

    public class ExampleCommand : BaseCommand
    {
        public ExampleCommand() : base("example1", "")
        {
        }

        public override bool Execute(ExecutedCommand command, bool partOfMultiCommand)
        {
            ConsoleWindow3.SendConsoleResponse("An example command from ExampleMod1", ConsoleMessageType.SpecialInfo);
            return true;
        }
    }
}