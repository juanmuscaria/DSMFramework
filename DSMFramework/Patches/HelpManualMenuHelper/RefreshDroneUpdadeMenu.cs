using DSMFramework.Modding;
using HarmonyLib;

namespace DSMFramework.Patches.HelpManualMenuHelper
{
    [HarmonyPatch(typeof(global::HelpManualMenuHelper))]
    [HarmonyPatch("RefreshDroneUpdadeMenu")]
    public class RefreshDroneUpdadeMenu
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Local
        public static void Postfix(global::HelpManualMenuHelper __instance, bool ___useSimpleHelp, HelpManualMenu ___droneUpgrades) {
            ModHelpMenuManager.Manager.RefreshDroneUpgradeMenu(__instance, ___useSimpleHelp, ___droneUpgrades);
        }
    }
}