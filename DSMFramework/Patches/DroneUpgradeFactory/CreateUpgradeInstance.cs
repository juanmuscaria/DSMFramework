using DSMFramework.Modding;
using HarmonyLib;

//TODO: Broken
namespace DSMFramework.Patches.DroneUpgradeFactory
{
    [HarmonyPatch(typeof(global::DroneUpgradeFactory))]
    [HarmonyPatch("CreateUpgradeInstance", typeof(DroneUpgradeType), typeof(int))]
    public class CreateUpgradeInstance
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Local
        private static bool Prefix(ref DroneUpgradeType type, int id, ref BaseDroneUpgrade __result)
        {
            //Handle modded drone upgrades
            if ((int) type < ModUpgradeManager.BaseModdedType ||
                ModUpgradeManager.Manager._alreadyTaken.Contains((int) type))
                return
                    true; //not modded (or from some mod that did not used our framework), continue with the old method
            var upgrade = ModUpgradeManager.Manager.FromId((int) type); //try to get upgrade from id
            if (upgrade == null)
            {
                //invalid or removed mod, convert it to a generator
                type = DroneUpgradeType.Generator;
                return true; //continue the execution of the method
            }

            __result = upgrade.MakeUpgrade(); //make a modded upgrade instance
            if (id == -1) //Random upgrade generation
            {
                //Get the next available id for save file
                __result.Id = UniverseSaveFile.Get("LAST_DU_ID", 1); //set galaxy file unique id, different from type id
                UniverseSaveFile.Save("LAST_DU_ID", __result.Id + 1);
            }
            else
            {
                __result.Id = id; //set galaxy file unique id, different from type id
            }
            return false; //stop the execution and use our modded upgrade

        }
    }
}