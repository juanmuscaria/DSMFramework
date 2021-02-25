using HarmonyLib;
using ModLoader.Modding;

namespace ModLoader.Patches.GameplayManager
{
    [HarmonyPatch(typeof(global::GameplayManager))]
    [HarmonyPatch("Start")]
    public class Start
    {
        static void Prefix(global::GameplayManager __instance)
        {
            ConsoleWindow3.Instance.AddCommandableObject(CommandManager.Manager);
        }

        static void Postfix(global::GameplayManager __instance, GameEventManager ___gameEventManager)
        {
            ModGameEventManager.Manager.InjectEventsInto(___gameEventManager);
        }
    }
}