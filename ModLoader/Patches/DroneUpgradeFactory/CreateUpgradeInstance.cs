using System;
using HarmonyLib;
using ModLoader.Modding;

namespace ModLoader.Patches.DroneUpgradeFactory
{
    [HarmonyPatch(typeof(global::DroneUpgradeFactory))]
    [HarmonyPatch("CreateUpgradeInstance", typeof(DroneUpgradeType), typeof(int))]
    public class CreateUpgradeInstance
    {
        static bool Prefix(ref DroneUpgradeType type, int id, ref BaseDroneUpgrade __result)
        {
            //Handle modded drone upgrades
            if ((int) type >= ModUpgradeManager.BASE_MODDED_TYPE) 
            {
                var upgrade = ModUpgradeManager.Manager.FromId((int) type); //try to get upgrade from id
                if (upgrade == null)
                {
                    //invalid or removed mod, convert it to a generator
                    type = DroneUpgradeType.Generator;
                    return true; //continue the execution of the method
                }

                __result = upgrade.MakeUpgrade(); //make a modded upgrade intance
                __result.Id = id; //set galaxy file unique id, different from type id
                return false; //stop the execution and use our modded upgrade
            }
            return true; //not modded, continue with the old method
        }
        
    }
}