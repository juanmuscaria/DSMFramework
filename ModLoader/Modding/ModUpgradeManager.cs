using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModLoader.Modding
{
    public class ModUpgradeManager
    {
        public const int BASE_MODDED_TYPE = (int) DroneUpgradeType.NumberOfUpgrades + 1;
        public static readonly ModUpgradeManager Manager = new ModUpgradeManager();
        private readonly List<ModDroneUpgradeContainer> addedDroneUpgrades = new List<ModDroneUpgradeContainer>();

        private readonly Dictionary<int, ModDroneUpgradeContainer> mappedDroneUpgrades =
            new Dictionary<int, ModDroneUpgradeContainer>();

        private readonly Dictionary<Type, List<IModification>> modificationsByType =
            new Dictionary<Type, List<IModification>>();

        private readonly Dictionary<string, int> previousMappedDroneUpgrades = new Dictionary<string, int>();
        private bool frozen;
        private int nextId = BASE_MODDED_TYPE;

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

        /// <summary>
        ///     Gets the total amount of upgrade in the game counting normal ones and modded ones
        ///     This method is used internally
        /// </summary>
        /// <returns>The total amount of upgrades</returns>
        public int GetUpgradeCount()
        {
            return nextId - 1;
        }

        public void InjectModifications(Dictionary<Type, List<IModification>> target)
        {
            modificationsByType.ToList().ForEach(x => target.Add(x.Key, x.Value));
        }

        public void InjectDefinitions(List<DroneUpgradeDefinition> definitions)
        {
            foreach (var upgrade in mappedDroneUpgrades) definitions.Add(upgrade.Value.GetDefinition());
        }

        /// <summary>
        ///     Registers an upgrade in the game
        /// </summary>
        /// <param name="upgradeContainer">The container of the upgrade</param>
        /// <exception cref="InvalidOperationException">
        ///     Will be thrown if the game data is already frozen
        ///     This method can be only called during mod loading
        /// </exception>
        public void RegisterDroneUpgrade(ModDroneUpgradeContainer upgradeContainer)
        {
            if (frozen)
                throw new InvalidOperationException(
                    "Game data already frozen! This method can only be called during mod loading.");
            addedDroneUpgrades.Add(upgradeContainer);
        }

        /// <summary>
        ///     Register a modification that can be brought with scraps in the modification menu
        /// </summary>
        /// <param name="modificationTarget">The type of something that can be modified (drone upgrade for example)</param>
        /// <param name="modification">An instance of the modification</param>
        public void RegisterModificationFor(Type modificationTarget, IModification modification)
        {
            if (!modificationsByType.ContainsKey(modificationTarget))
                modificationsByType[modificationTarget] = new List<IModification>();
            modificationsByType[modificationTarget].Add(modification);
        }

        /// <summary>
        ///     Get a mod upgrade from the type id
        /// </summary>
        /// <param name="id">The id of it</param>
        /// <returns>The upgrade, null if it does not exist</returns>
        public ModDroneUpgradeContainer FromId(int id)
        {
            return mappedDroneUpgrades.ContainsKey(id) ? mappedDroneUpgrades[id] : null;
        }

        internal void Freeze()
        {
            frozen = true;
            foreach (var upgrade in addedDroneUpgrades)
                if (previousMappedDroneUpgrades.ContainsKey(upgrade.name))
                {
                    var id = previousMappedDroneUpgrades[upgrade.name]; //get the previously registered id
                    upgrade.Register(id); //tells the upgrade to create it's definition
                    mappedDroneUpgrades[id] = upgrade; //map it for id>upgrade for upgrade factory
                }
                else
                {
                    var id = nextId++; //get a new id
                    //save it for reusing the next game restart so upgrades can be loaded from game save file
                    previousMappedDroneUpgrades[upgrade.name] = id;
                    upgrade.Register(id); //tells the upgrade to create it's definition
                    mappedDroneUpgrades[id] = upgrade; //map it for id>upgrade for upgrade factory
                }

            previousMappedDroneUpgrades["nextId"] = nextId; //save the next available id 
            //Save it into a file
            XmlHelper.ObjectToFile(previousMappedDroneUpgrades,
                Path.Combine(GameFileHelper.GetBaseGameFileLocation(), "modloader-droneUpgradeData.xml"));
        }
    }

    /// <summary>
    ///     Modded drone upgrades must be wrapped for data persistence, this class helps with the hard part of it
    /// </summary>
    public abstract class ModDroneUpgradeContainer
    {
        public readonly int cost;
        public readonly string description;
        public readonly int duration;
        public readonly int modifier;
        public readonly string name;
        public readonly DroneUpgradeClass upgradeClass;
        protected DroneUpgradeDefinition myDefinition;

        /// <summary>
        ///     Creates a new container with the following properties
        /// </summary>
        /// <param name="name">The name of this upgrade</param>
        /// <param name="description">The description of this upgrade</param>
        /// <param name="cost">How much it costs to buy from auto trader, all upgrades in the games always uses 3</param>
        /// <param name="modifier">The modifier this upgrade applies to a drone, currently used by speed upgrade</param>
        /// <param name="duration">Duration of the upgrade, used by upgrades like stealthy, sonic, shield</param>
        /// <param name="upgradeClass">The class the upgrade belongs to</param>
        protected ModDroneUpgradeContainer(string name, string description, int cost, int modifier, int duration,
            DroneUpgradeClass upgradeClass)
        {
            this.name = name;
            this.description = description;
            this.cost = cost;
            this.modifier = modifier;
            this.duration = duration;
            this.upgradeClass = upgradeClass;
        }

        /// <summary>
        ///     Creates a new container with the following properties
        /// </summary>
        /// <param name="name">The name of this upgrade</param>
        /// <param name="description">The description of this upgrade</param>
        /// <param name="upgradeClass">The class the upgrade belongs to</param>
        protected ModDroneUpgradeContainer(string name, string description, DroneUpgradeClass upgradeClass)
        {
            this.name = name;
            this.description = description;
            this.upgradeClass = upgradeClass;
            cost = 3;
            modifier = 0;
            duration = 0;
        }

        /// <summary>
        ///     Creates the definition of this upgrade, used internally
        /// </summary>
        /// <param name="id">
        ///     The type id of this upgrade, random at the first time the mod is loaded,
        ///     but consistent after restarts
        /// </param>
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

        /// <summary>
        ///     Creates a new instance of a modded drone upgrade, must be implemented and use the internal definition
        /// </summary>
        /// <returns>Must returns a new modded drone upgrade using the registered definition</returns>
        public abstract BaseDroneUpgrade MakeUpgrade();

        /// <summary>
        ///     The definition of this drone upgrade, may be null if called before the game is fully loaded
        /// </summary>
        /// <returns>The definition of this upgrade</returns>
        public DroneUpgradeDefinition GetDefinition()
        {
            return myDefinition;
        }
    }
}