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
            ModUpgradeManager.Manager.RegisterDroneUpgrade(new DoorChargerUpgradeContainer());
            ModUpgradeManager.Manager.RegisterModificationFor(typeof(DoorChargerUpgrade),new AddChargeMod());
        }
    }
}