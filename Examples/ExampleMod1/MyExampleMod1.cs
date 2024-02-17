
using DSMFramework.Modding;
using BepInEx;

namespace ExampleMod1
{
    // This will be the entry point of the mod
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        //This method is called when your mod is being loaded
        private void Awake()
        {
            //Add a new command to the game
            ModCommandManager.Manager.AddCommand(new ExampleCommand());
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