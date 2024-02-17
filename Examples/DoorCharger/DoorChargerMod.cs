using DoorCharger.Upgrade;
using DSMFramework.Modding;
using BepInEx;

namespace DoorCharger
{
    // This will be the entry point of the mod
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        //This method is called when your mod is being loaded
        private void Awake()
        {
            //Registers our upgrade in the game
            ModUpgradeManager.Manager.RegisterDroneUpgrade(new DoorChargerUpgradeContainer());
            //Adds a modification for the modification menu allowing player to buy more chargers
            ModUpgradeManager.Manager.RegisterModificationFor(typeof(DoorChargerUpgrade), new AddChargeMod());
        }
    }
}