using System.Collections.Generic;

namespace DSMFramework.Modding
{
    public class ModHelpMenuManager
    {
        public static readonly ModHelpMenuManager Manager = new ModHelpMenuManager();
        private ModHelpMenuManager()
        {
            
        }

        internal static void RefreshDroneUpgradeMenu(HelpManualMenuHelper menuHelper, bool useSimpleHelp, HelpManualMenu droneMenu)
        {
            foreach (var modDroneUpgrade in ModUpgradeManager.Manager.GETAllModUpgrades())
            {
                if (GlobalSettings.DiscoveredUpgrades.Contains(modDroneUpgrade.GetDefinition().Type) ||
                     useSimpleHelp)
                {
                    menuHelper.AddCommands(droneMenu.MenuItems, modDroneUpgrade.GetCommandDefinitions());
                }
            }
        }

        internal static void AddModCommands(HelpManualMenuHelper menuHelper)
        {
            var basicCommands = menuHelper.GetFirstMenu().MenuItems["0"];
            var advancedCommands = menuHelper.GetFirstMenu().MenuItems["2"];
            var toAdd = new List<CommandDefinition>();
            var toAddAdv = new List<CommandDefinition>();
            foreach (var command in ModCommandManager.Manager.QueryAvailableCommands())
            {
                if (command.IsAdvanced)
                    toAddAdv.Add(command);
                else
                    toAdd.Add(command);
            }
            menuHelper.AddCommands(basicCommands.JumpToMenu.MenuItems, toAdd);
            menuHelper.AddCommands(advancedCommands.JumpToMenu.MenuItems, toAddAdv, true);
        }
    }
}