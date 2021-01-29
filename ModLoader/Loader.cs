using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using HarmonyLib;
using ModLoader.Modding;
using UnityEngine;

namespace ModLoader
{
    public class Loader
    {
        public static readonly List<IMod> Mods = new List<IMod>();
        
        //Called from Steamworks.SteamAPI#Init() method
        public static void Start()
        {
            //Load our patches
            var harmony = new Harmony("com.juanmuscaria.ModLoader"); 
            harmony.PatchAll(); 
            
            //Start loading other mods
            var modFolder = GETModFolder();
            Debug.Log("Searching '{}' for mods.".Replace("{}",modFolder.FullName));

            foreach (var modFile in Directory.GetFiles(modFolder.FullName, "*.dll"))
            {
                Debug.Log("Loading mod file:" + modFile);
                foreach (var type in Assembly.LoadFile(modFile).GetExportedTypes())
                {
                    try
                    {
                        if (!type.IsDefined(typeof(IMod), true) &&
                            !typeof(IMod).IsAssignableFrom(type)) continue;
                        Debug.Log("Found '"+ type + "' in '" + modFile + "'");
                        var ctor = type.GetConstructor(Type.EmptyTypes);
                        if (ctor != null)
                        {
                            Mods.Add((IMod) ctor.Invoke(null));
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Failed to load '"+ type + "' in '" + modFile + "'");
                        Debug.LogError(ex);
                    }
                } 
            }

            foreach (var mod in Mods)
            {
                mod.Load();
            }
            
            CommandManager.Manager.AddCommand(new ModloaderCommand());
        }


        //Get the mod directory
        private static DirectoryInfo GETModFolder()
        {
            var directoryInfo = new DirectoryInfo(Application.dataPath).Parent;
            if (directoryInfo != null)
            {
                string path = directoryInfo.FullName;
                if (SystemInfo.operatingSystem.Contains("Windows"))
                {
                    path =  path + "\\Mods";
                }
                else //Assume linux
                {
                    path = path + "/Mods";
                }

                return new DirectoryInfo(path);
            }

            throw new ModloaderException("Unable to determine the mod folder.");
        }
    }
    
    [Serializable]
    public class ModloaderException : Exception
    {
        public ModloaderException() { }
        public ModloaderException(string message) : base(message) { }
        public ModloaderException(string message, Exception inner) : base(message, inner) { }
        
        protected ModloaderException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
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
                ConsoleWindow3.SendConsoleResponse("Modloader v0.0.0 by juanmuscaria", ConsoleMessageType.SpecialInfo);
                ConsoleWindow3.SendConsoleResponse("Use modloader mods to list all installed mods", ConsoleMessageType.SpecialInfo);
            }
            else
            {
                if (command.Arguments[0].ToLower().Equals("mods"))
                {
                    ConsoleWindow3.SendConsoleResponse("Modloader v0.0.0 by juanmuscaria", ConsoleMessageType.SpecialInfo);
                    if (Loader.Mods.Count > 0)
                    {
                        foreach (var mod in Loader.Mods)
                        {
                            ConsoleWindow3.SendConsoleResponse("--------------------", ConsoleMessageType.SpecialInfo);
                            ConsoleWindow3.SendConsoleResponse("Mod name: " + mod.GetType().Assembly.GetName().Name, ConsoleMessageType.SpecialInfo);
                            ConsoleWindow3.SendConsoleResponse("Mod description: <still not implemented>" , ConsoleMessageType.SpecialInfo);
                            ConsoleWindow3.SendConsoleResponse("Mod version: " + mod.GetType().Assembly.GetName().Version, ConsoleMessageType.SpecialInfo);
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
                    ConsoleWindow3.SendConsoleResponse("Modloader v0.0.0 by juanmuscaria", ConsoleMessageType.SpecialInfo);
                    ConsoleWindow3.SendConsoleResponse("Unknown subcommand " + command.Arguments[0].ToLower(), ConsoleMessageType.Error);
                }
            }

            return true;
        }
    }
}