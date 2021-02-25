using HarmonyLib;
using UnityEngine;

namespace ModLoader.Patches.GameFileHelper
{
    [HarmonyPatch(typeof(global::GameFileHelper))]
    [HarmonyPatch("GetBaseGameFileLocation")]
    public class GetBaseGameFileLocation
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Local
        private static void Postfix(ref string __result)
        {
            __result = Application.persistentDataPath + Loader.UNIQUE_MODLOADER_IDENT;
        }
    }
}