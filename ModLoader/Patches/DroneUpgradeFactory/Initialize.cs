using System.Collections.Generic;
using HarmonyLib;
using ModLoader.Modding;

namespace ModLoader.Patches.DroneUpgradeFactory
{
    [HarmonyPatch(typeof(global::DroneUpgradeFactory))]
    [HarmonyPatch("Initialize")]
    public class Initialize
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Local
        private static void Prefix(List<DroneUpgradeDefinition> ____upgradeDefinitions, bool ____initialized)
        {
            if (____initialized)
                return;
            ModUpgradeManager.Manager.InjectDefinitions(____upgradeDefinitions);
        }
    }
}