using DoorCharger.Upgrade;
using ModLoader.Modding;

namespace DoorCharger
{
    [ModInfo("DoorCharger",
        "Door Charger",
        "1.0.*",
        description = "A mod that adds a new drone upgrade")]
    public class DoorChargerMod : Mod
    {
        public override void Load()
        {
            //Registers our upgrade in the game
            ModUpgradeManager.Manager.RegisterDroneUpgrade(new DoorChargerUpgradeContainer());
            //Adds a modification for the modification menu allowing player to buy more chargers
            ModUpgradeManager.Manager.RegisterModificationFor(typeof(DoorChargerUpgrade), new AddChargeMod());
        }
    }
}