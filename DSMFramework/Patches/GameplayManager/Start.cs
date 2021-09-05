using DSMFramework.Modding;
using HarmonyLib;

namespace DSMFramework.Patches.GameplayManager
{
    [HarmonyPatch(typeof(global::GameplayManager))]
    [HarmonyPatch("Start")]
    public class Start
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Local
        private static void Prefix(global::GameplayManager __instance)
        {
            ConsoleWindow3.Instance.AddCommandableObject(CommandManager.Manager);
        }
        
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once InconsistentNaming
        private static void Postfix(global::GameplayManager __instance, GameEventManager ___gameEventManager)
        {
            ModGameEventManager.Manager.InjectEventsInto(___gameEventManager);
        }
    }
}