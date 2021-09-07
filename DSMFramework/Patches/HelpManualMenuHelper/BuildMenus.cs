using System.Collections.Generic;
using DSMFramework.Modding;
using HarmonyLib;

namespace DSMFramework.Patches.HelpManualMenuHelper
{
    [HarmonyPatch(typeof(global::HelpManualMenuHelper))]
    [HarmonyPatch("BuildMenus",typeof(bool))]
    public class BuildMenus
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Local
        public static void Postfix(global::HelpManualMenuHelper __instance)
        {
            ModHelpMenuManager.Manager.AddModCommands(__instance);
        }
    }
}