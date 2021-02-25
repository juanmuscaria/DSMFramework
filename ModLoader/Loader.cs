using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using HarmonyLib;
using ModLoader.Modding;
using UnityEngine;

namespace ModLoader
{
    public class Loader
    {
        public const string MOD_LOADER_VERSION = "0.2.*";
        public const string UNIQUE_MODLOADER_IDENT = "-Modded";
        private static readonly List<Mod> FoundMods = new List<Mod>();
        public static readonly List<Mod> LoadedMods = new List<Mod>();
        internal static bool Tainted;

        //Called from Steamworks.SteamAPI#Init() method
        public static void Start()
        {
            //Load our patches
            var harmony = new Harmony("com.juanmuscaria.ModLoader");
            harmony.PatchAll();

            //Start loading other mods
            var modFolder = GETModFolder();
            Debug.Log($"Searching '{modFolder.FullName}' for mods.");

            foreach (var modFile in Directory.GetFiles(modFolder.FullName, "*.dll"))
            {
                Debug.Log($"Loading mod file: {modFile}");
                foreach (var type in Assembly.LoadFile(modFile).GetExportedTypes())
                    if (ProcessType(type))
                        break;
            }

            //Sort the mod loading order
            FoundMods.Sort();
            foreach (var foundMod in FoundMods)
                try
                {
                    foundMod.Load();
                    if (foundMod.Taint())
                        Tainted = true;
                    LoadedMods.Add(foundMod);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load {foundMod}");
                    Debug.LogError(e);
                }

            //Finish loading some stuff and freeze modloader registry
            CommandManager.Manager.AddCommand(new ModloaderCommand());
            ModUpgradeManager.Manager.Freeze();
        }

        private static bool ProcessType(Type type)
        {
            try
            {
                if (!typeof(Mod).IsAssignableFrom(type))
                    return false;
                if (!type.IsDefined(typeof(ModInfo), true))
                {
                    Debug.LogWarning($"Mod '{type}' has no ModInfo attribute! Skipping it...");
                    return false;
                }

                var ctor = type.GetConstructor(Type.EmptyTypes);
                if (ctor != null) FoundMods.Add((Mod) ctor.Invoke(null));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load {type}");
                Debug.LogError(ex);
            }

            return false;
        }

        //Get the mod directory
        private static DirectoryInfo GETModFolder()
        {
            var directoryInfo = new DirectoryInfo(Application.dataPath).Parent;
            if (directoryInfo != null) return new DirectoryInfo(Path.Combine(directoryInfo.FullName, "Mods"));
            throw new ModloaderException("Unable to determine the mod folder.");
        }
    }

    [Serializable]
    public class ModloaderException : Exception
    {
        public ModloaderException()
        {
        }

        public ModloaderException(string message) : base(message)
        {
        }

        public ModloaderException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ModloaderException(SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }

    public class ModloaderCommand : BaseCommand
    {
        public ModloaderCommand() : base("modloader", "modloader")
        {
        }

        public override bool Execute(ExecutedCommand command, bool partOfMultiCommand)
        {
            if (command.Arguments.Count == 0)
            {
                ConsoleWindow3.SendConsoleResponse($"Modloader v{Loader.MOD_LOADER_VERSION} by juanmuscaria",
                    ConsoleMessageType.SpecialInfo);
                ConsoleWindow3.SendConsoleResponse("Use modloader mods to list all installed mods",
                    ConsoleMessageType.SpecialInfo);
            }
            else
            {
                if (command.Arguments[0].ToLower().Equals("mods"))
                {
                    ConsoleWindow3.SendConsoleResponse($"Modloader v{Loader.MOD_LOADER_VERSION} by juanmuscaria",
                        ConsoleMessageType.SpecialInfo);
                    if (Loader.LoadedMods.Count > 0)
                    {
                        foreach (var mod in Loader.LoadedMods)
                        {
                            var info = ModInfo.OfMod(mod);
                            ConsoleWindow3.SendConsoleResponse("--------------------", ConsoleMessageType.SpecialInfo);
                            ConsoleWindow3.SendConsoleResponse($"Mod name: {info.name}",
                                ConsoleMessageType.SpecialInfo);
                            ConsoleWindow3.SendConsoleResponse($"Mod description: {info.description}",
                                ConsoleMessageType.SpecialInfo);
                            ConsoleWindow3.SendConsoleResponse($"Mod version: {info.version}",
                                ConsoleMessageType.SpecialInfo);
                        }

                        ConsoleWindow3.SendConsoleResponse("--------------------", ConsoleMessageType.SpecialInfo);
                    }
                    else
                    {
                        ConsoleWindow3.SendConsoleResponse("You have no mod installed :(", ConsoleMessageType.Error);
                    }
                }
                else
                {
                    ConsoleWindow3.SendConsoleResponse($"Modloader v{Loader.MOD_LOADER_VERSION} by juanmuscaria",
                        ConsoleMessageType.SpecialInfo);
                    ConsoleWindow3.SendConsoleResponse("Unknown subcommand " + command.Arguments[0].ToLower(),
                        ConsoleMessageType.Error);
                }
            }

            return true;
        }
    }
}