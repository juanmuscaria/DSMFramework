using ModLoader.Modding;

namespace ExampleMod1
{
    // This will be the entry point of the mod, you must extend IMod
    [ModInfo("ExampleMod1",
        "Example Mod 1",
        "1.0.*",
        description = "A simple mod that adds example1 command")]
    public class MyExampleMod1 : Mod
    {
        //This method is called when your mod is being loaded
        public override void Load()
        {
            //Add a new command to the game
            CommandManager.Manager.AddCommand(new ExampleCommand());
        }
    }

    public class ExampleCommand : BaseCommand
    {
        public ExampleCommand() : base("example1", "")
        {
        }

        //This method is called when your command is executed
        //Return true if the command was handled.
        //Returning false will continue the search for any other command matching the same name as yours
        public override bool Execute(ExecutedCommand command, bool partOfMultiCommand)
        {
            ConsoleWindow3.SendConsoleResponse("An example command from ExampleMod1", ConsoleMessageType.SpecialInfo);
            return true;
        }
    }
}