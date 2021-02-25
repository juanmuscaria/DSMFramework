using HarmonyLib;
using UnityEngine;

namespace ModLoader.Patches.GameFileHelper
{
    [HarmonyPatch(typeof(global::GameFileHelper))]
    [HarmonyPatch("GetBaseGameFileLocation")]
    public class GetBaseGameFileLocation
    {
        static void Postfix(ref string __result)
        {
            __result = Application.persistentDataPath + Loader.UNIQUE_MODLOADER_IDENT;
        }
    }
}