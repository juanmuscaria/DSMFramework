using System;
using System.Collections.Generic;
using HarmonyLib;
using ModLoader.Modding;

namespace ModLoader.Patches.ModificationsHelper
{
    [HarmonyPatch(typeof(global::ModificationsHelper))]
    [HarmonyPatch("LoadModifications")]
    public class LoadModifications
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Local
        private static void Postfix(Dictionary<Type, List<IModification>> ____modificationsByType)
        {
            ModUpgradeManager.Manager.InjectModifications(____modificationsByType);
        }
    }
}