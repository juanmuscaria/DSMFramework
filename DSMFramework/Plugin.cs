using BepInEx;
using DSMFramework.Modding;
using HarmonyLib;

namespace DSMFramework
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource LOGGER;
        private void Awake()
        {
            LOGGER = Logger;
            var harmony = new Harmony("com.juanmuscaria.DSMFramework");
            harmony.PatchAll();
            ModCommandManager.Manager.AddCommand(new DsmfCommand());
            Logger.LogInfo("Registered all Harmony patches");
        }
    }

    public class DsmfCommand : BaseCommand
    {
        public DsmfCommand() : base("dsmf", "dsmf <subcommand>")
        {
            DetailedDescription.Add(new ConsoleMessage("Duskers Simple Modding Framework command interface", ConsoleMessageType.Info));
            DetailedDescription.Add(new ConsoleMessage("Available subcommands:", ConsoleMessageType.Info));
            DetailedDescription.Add(new ConsoleMessage("\t- testdronerng: Spawns a lootable drone in every room.", ConsoleMessageType.Info));
        }

        public override bool Execute(ExecutedCommand command, bool partOfMultiCommand)
        {
            if (command.Arguments.Count == 0)
            {
                ConsoleWindow3.SendConsoleResponse($"Duskers Simple Modding Framework v{PluginInfo.PLUGIN_VERSION} by juanmuscaria",
                    ConsoleMessageType.SpecialInfo);
            }
            else
            {
                if (command.Arguments[0].ToLower().Equals("testdronerng") && GlobalSettings.cheatMode)
                {
                    int id = 4 + DroneManager.Instance.LootableDronesList.Count;
                    foreach (Room room in DungeonManager.Instance.rooms)
                    {
                        DroneManager.Instance.PlaceLootableDroneInRoom(room, ref id, true);
                        id++;
                    }
                }
            }

            return true;
        }
    }
}
