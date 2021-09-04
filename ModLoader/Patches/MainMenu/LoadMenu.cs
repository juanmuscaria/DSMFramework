using System;
using HarmonyLib;
using UnityEngine;

namespace ModLoader.Patches.MainMenu
{
    [HarmonyPatch(typeof(global::MainMenu))]
    [HarmonyPatch("LoadMenu")]
    public class LoadMenu
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Local
        private static void Prefix(global::MainMenu __instance)
        {
            
        }

        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once InconsistentNaming
        private static void Postfix(global::MainMenu __instance)
        {
            MenuPanelUI.Instance.AddMenuItem(new DuskersMenuItem("Mod[L]oader",KeyCode.L, ModsButton, 10));
        }

        private static void ModsButton()
        {
            MenuPanelUI.Instance.Clear();
            new ModsMenu();
        }
    }
}