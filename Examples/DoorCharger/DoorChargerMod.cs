using DoorCharger.Upgrade;
using DSMFramework.Modding;

namespace DoorCharger
{
    public static class DoorChargerMod
    {
        public static void Load()
        {
            //Registers our upgrade in the game
            ModUpgradeManager.Manager.RegisterDroneUpgrade(new DoorChargerUpgradeContainer());
            //Adds a modification for the modification menu allowing player to buy more chargers
            ModUpgradeManager.Manager.RegisterModificationFor(typeof(DoorChargerUpgrade), new AddChargeMod());
        }
    }
}