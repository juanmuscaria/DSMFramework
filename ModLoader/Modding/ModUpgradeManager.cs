using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace ModLoader.Modding
{
    public class ModUpgradeManager
    {
        
        public const int BASE_MODDED_TYPE = (int) DroneUpgradeType.NumberOfUpgrades + 1;
        private int nextId = BASE_MODDED_TYPE;
        private readonly Dictionary<Type, List<IModification>> modificationsByType = new Dictionary<Type, List<IModification>>();
        private readonly List<ModDroneUpgradeContainer> addedDroneUpgrades = new List<ModDroneUpgradeContainer>();
        private readonly Dictionary<string, int> previousMappedDroneUpgrades = new Dictionary<string, int>();
        private readonly Dictionary<int, ModDroneUpgradeContainer> mappedDroneUpgrades = new Dictionary<int, ModDroneUpgradeContainer>();
        public static readonly ModUpgradeManager Manager = new ModUpgradeManager();
        private bool frozen;

        public int GetUpgradeCount()
        {
            return nextId - 1;
        }
        private ModUpgradeManager()
        {
            var mapped = XmlHelper.FileToObject<Dictionary<string, int>>(
                Path.Combine(GameFileHelper.GetBaseGameFileLocation(), "modloader-droneUpgradeData.xml"));
            if (mapped != null)
            {
                previousMappedDroneUpgrades = mapped;
                nextId = mapped["nextId"];
            }
            
        }

        public void InjectModifications(Dictionary<Type, List<IModification>> target)
        {
            modificationsByType.ToList().ForEach(x => target.Add(x.Key, x.Value));
        }
        public void InjectDefinitions(List<DroneUpgradeDefinition> definitions)
        {
            foreach (var upgrade in mappedDroneUpgrades)
            {
                definitions.Add(upgrade.Value.GetDefinition());
            }
        }
        public void RegisterDroneUpgrade(ModDroneUpgradeContainer upgradeContainer)
        {
            if (frozen)
                throw new InvalidOperationException(
                    "Game data already frozen! This method can only be called during mod loading.");
            addedDroneUpgrades.Add(upgradeContainer);
        }

        public void RegisterModificationFor(Type modificationTarget, IModification modification)
        {
            if (!modificationsByType.ContainsKey(modificationTarget))
                modificationsByType[modificationTarget] = new List<IModification>();
            modificationsByType[modificationTarget].Add(modification);
        }

        public ModDroneUpgradeContainer FromId(int id)
        {
            return mappedDroneUpgrades.ContainsKey(id) ? mappedDroneUpgrades[id] : null;
        }
        internal void Freeze()
        {
            frozen = true;
            foreach (var upgrade in addedDroneUpgrades)
            {
                if (previousMappedDroneUpgrades.ContainsKey(upgrade.name))
                {
                    int id = previousMappedDroneUpgrades[upgrade.name]; //get the previously registered id
                    upgrade.Register(id); //tells the upgrade to create it's definition
                    mappedDroneUpgrades[id] = upgrade; //map it for id>upgrade for upgrade factory
                }
                else
                {
                    int id = nextId++; //get a new id
                    //save it for reusing the next game restart so upgrades can be loaded from game save file
                    previousMappedDroneUpgrades[upgrade.name] = id;
                    upgrade.Register(id); //tells the upgrade to create it's definition
                    mappedDroneUpgrades[id] = upgrade; //map it for id>upgrade for upgrade factory
                    
                }
            }
            
            previousMappedDroneUpgrades["nextId"] = nextId; //save the next available id 
            //Save it into a file
            XmlHelper.ObjectToFile(previousMappedDroneUpgrades, 
                Path.Combine(GameFileHelper.GetBaseGameFileLocation(), "modloader-droneUpgradeData.xml"));
        }
    }

    
    public abstract class ModDroneUpgradeContainer
    {
        protected DroneUpgradeDefinition myDefinition;
        public readonly string name;
        public readonly string description;
        public readonly int cost;
        public readonly int modifier;
        public readonly int duration;
        public readonly DroneUpgradeClass upgradeClass;
        
        protected ModDroneUpgradeContainer(string name, string description, int cost, int modifier, int duration, DroneUpgradeClass upgradeClass)
        {
            this.name = name;
            this.description = description;
            this.cost = cost;
            this.modifier = modifier;
            this.duration = duration;
            this.upgradeClass = upgradeClass;
        }
        
        protected ModDroneUpgradeContainer(string name, string description, DroneUpgradeClass upgradeClass)
        {
            this.name = name;
            this.description = description;
            this.upgradeClass = upgradeClass;
            cost = 3;
            modifier = 0;
            duration = 0;
        }

        internal void Register(int id)
        {
            myDefinition = new DroneUpgradeDefinition(id.ToString(), 
                "true", 
                name, 
                description, 
                "0", 
                "0", 
                cost.ToString(),
                modifier.ToString(),
                "0",
                duration.ToString(),
                upgradeClass.ToString());
        }

        public abstract BaseDroneUpgrade MakeUpgrade();

        public DroneUpgradeDefinition GetDefinition()
        {
            return myDefinition;
        }
    }
}