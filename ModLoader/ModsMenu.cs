using ModLoader.Modding;
using UnityEngine;

namespace ModLoader
{
    public class ModsMenu : MenuScreenClass
    {
        public static ModsMenu Instance;

        public ModsMenu()
        {
            Instance = this;
        }

        protected override void Initialize()
        {
            ActiveText = "Loaded Mods";
            IgnoreCancel = false;
            base.Initialize();
        }

        public override void LoadMenu()
        {
            if (Loader.LoadedMods.Count == 0)
            {
                MenuPanelUI.Instance.AddMenuItem(new DuskersMenuItem("No loaded mods :(", KeyCode.None, null, 0));
            }
            else
            {
                int menuItem = 0;
                foreach (var mod in Loader.LoadedMods)
                {
                    var info = ModInfo.OfMod(mod);
                    MenuPanelUI.Instance.AddMenuItem(new DuskersMenuItem(info.name, KeyCode.None, null , menuItem));
                    menuItem++;
                }
            }
            
        }
    }
}