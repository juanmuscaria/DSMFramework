using System.Collections.Generic;
using DSMFramework.Modding;
using HarmonyLib;

namespace DSMFramework.Patches.DroneUpgradeFactory
{
    [HarmonyPatch(typeof(global::DroneUpgradeFactory))]
    [HarmonyPatch("LoadUpgradeDefinitionLibrary")]
    public class LoadUpgradeDefinitionLibrary
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Local
        private static void Postfix(List<DroneUpgradeDefinition> ____upgradeDefinitions)
        {
            Plugin.LOGGER.LogMessage("LoadUpgradeDefinitionLibrary");
            ModUpgradeManager.Manager.MapAndAddModdedDroneDefinitions(____upgradeDefinitions);
            ModUpgradeManager.Manager.InjectDefinitions(____upgradeDefinitions);
        }
    }
}