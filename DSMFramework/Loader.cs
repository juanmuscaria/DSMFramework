using DSMFramework.Modding;
using HarmonyLib;
using UnityModManagerNet;

namespace DSMFramework
{
    public static class Loader
    {
        public const string ModLoaderVersion = "0.1.*";
        public const string UniqueModloaderIdent = "-Modded";

        
        static void Load(UnityModManager.ModEntry modEntry) 
        {
            //Load our patches
            var harmony = new Harmony("com.juanmuscaria.DSMFramework");
            harmony.PatchAll();
            ModCommandManager.Manager.AddCommand(new DsmfCommand());
        }
        
    }
    
    public class DsmfCommand : BaseCommand
    {
        public DsmfCommand() : base("dsmf", "dsmf <subcommand>")
        {
            DetailedDescription.Add(new ConsoleMessage("Duskers Simple Modding Framework command interface", ConsoleMessageType.Info));
            DetailedDescription.Add(new ConsoleMessage("Available subcommands:", ConsoleMessageType.Info));
            DetailedDescription.Add(new ConsoleMessage("\t- mods: List all loaded mods.", ConsoleMessageType.Info));
        }

        public override bool Execute(ExecutedCommand command, bool partOfMultiCommand)
        {
            if (command.Arguments.Count == 0)
            {
                ConsoleWindow3.SendConsoleResponse($"Duskers Simple Modding Framework v{Loader.ModLoaderVersion} by juanmuscaria",
                    ConsoleMessageType.SpecialInfo);
                ConsoleWindow3.SendConsoleResponse("Use dsmf mods to list all loaded mods",
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
                // if (command.Arguments[0].ToLower().Equals("mods"))
                // {
                //     ConsoleWindow3.SendConsoleResponse($"Modloader v{Loader.MOD_LOADER_VERSION} by juanmuscaria",
                //         ConsoleMessageType.SpecialInfo);
                //     if (Loader.LoadedMods.Count > 0)
                //     {
                //         foreach (var mod in Loader.LoadedMods)
                //         {
                //             var info = ModInfo.OfMod(mod);
                //             ConsoleWindow3.SendConsoleResponse("--------------------", ConsoleMessageType.SpecialInfo);
                //             ConsoleWindow3.SendConsoleResponse($"Mod name: {info.name}",
                //                 ConsoleMessageType.SpecialInfo);
                //             ConsoleWindow3.SendConsoleResponse($"Mod description: {info.description}",
                //                 ConsoleMessageType.SpecialInfo);
                //             ConsoleWindow3.SendConsoleResponse($"Mod version: {info.version}",
                //                 ConsoleMessageType.SpecialInfo);
                //         }
                //
                //         ConsoleWindow3.SendConsoleResponse("--------------------", ConsoleMessageType.SpecialInfo);
                //     }
                //     else
                //     {
                //         ConsoleWindow3.SendConsoleResponse("You have no mod installed :(", ConsoleMessageType.Error);
                //     }
                // }
                else
                {
                    ConsoleWindow3.SendConsoleResponse($"Modloader v{Loader.ModLoaderVersion} by juanmuscaria",
                        ConsoleMessageType.SpecialInfo);
                    ConsoleWindow3.SendConsoleResponse("Unknown subcommand " + command.Arguments[0].ToLower(),
                        ConsoleMessageType.Error);
                }
            }

            return true;
        }
    }
}