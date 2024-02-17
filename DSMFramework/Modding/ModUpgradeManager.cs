using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DSMFramework.Modding
{
    public class ModUpgradeManager
    {
        public static readonly ModUpgradeManager Manager = new ModUpgradeManager();

        public const int BaseModdedType = (int) DroneUpgradeType.NumberOfUpgrades + 1;
        private readonly List<ModDroneUpgradeContainer> _addedDroneUpgrades = new List<ModDroneUpgradeContainer>();

        private readonly Dictionary<int, ModDroneUpgradeContainer> _mappedDroneUpgrades =
            new Dictionary<int, ModDroneUpgradeContainer>();

        private readonly Dictionary<Type, List<IModification>> _modificationsByType =
            new Dictionary<Type, List<IModification>>();

        private Dictionary<string, int> _previousMappedDroneUpgrades = new Dictionary<string, int>();
        private bool _frozen;
        private int _nextId = BaseModdedType;
        internal List<int> _alreadyTaken = new List<int>();

        private ModUpgradeManager()
        {
        }

        /// <summary>
        ///     Gets the total amount of upgrade in the game counting normal ones and modded ones
        ///     This method is used internally
        /// </summary>
        /// <returns>The total amount of upgrades</returns>
        public int GetUpgradeCount()
        {
            return _nextId - 1;
        }

        public void InjectModifications(Dictionary<Type, List<IModification>> target)
        {
            _modificationsByType.ToList().ForEach(x =>
            {
                if (target.ContainsKey(x.Key))
                {
                    target[x.Key] = target[x.Key].Concat(x.Value).ToList();
                }
                else
                {
                    target.Add(x.Key, x.Value);
                }
            });
        }

        public void InjectDefinitions(List<DroneUpgradeDefinition> definitions)
        {
            definitions.AddRange(_mappedDroneUpgrades.Select(upgrade => upgrade.Value.GetDefinition()));
        }

        /// <summary>
        ///     Registers an upgrade in the game
        /// </summary>
        /// <param name="upgradeContainer">The container of the upgrade</param>
        /// <exception cref="InvalidOperationException">
        ///     Will be thrown if the game is already making use of droge upgrades
        ///     This method can be only called during mod loading
        /// </exception>
        public void RegisterDroneUpgrade(ModDroneUpgradeContainer upgradeContainer)
        {
            if (_frozen)
                throw new InvalidOperationException(
                    "Drone Upgrades are already in use by the game, try registering your upgrades before the game finishes loading.");
            _addedDroneUpgrades.Add(upgradeContainer);
        }

        /// <summary>
        ///     Register a modification that can be brought with scraps in the modification menu
        /// </summary>
        /// <param name="modificationTarget">The type of something that can be modified (drone upgrade for example)</param>
        /// <param name="modification">An instance of the modification</param>
        public void RegisterModificationFor(Type modificationTarget, IModification modification)
        {
            if (!_modificationsByType.ContainsKey(modificationTarget))
                _modificationsByType[modificationTarget] = new List<IModification>();
            _modificationsByType[modificationTarget].Add(modification);
        }

        /// <summary>
        ///     Get a mod upgrade from the type id
        /// </summary>
        /// <param name="id">The id of it</param>
        /// <returns>The upgrade, null if it does not exist</returns>
        public ModDroneUpgradeContainer FromId(int id)
        {
            return _mappedDroneUpgrades.ContainsKey(id) ? _mappedDroneUpgrades[id] : null;
        }

        internal void MapAndAddModdedDroneDefinitions(IEnumerable<DroneUpgradeDefinition> otherDefinitions)
        {
            if (_frozen) {
                return;
            }
            LoadMapping();
            _frozen = true;
            // checks if other mods that does not use the framework added their own upgrades.
            // To keep consistency if a mod added their on upgrade after an DSMF upgrade was already registered with such id they will be overwritten by us.
            foreach (var upgrade in otherDefinitions)
            {
                if (upgrade.Type > DroneUpgradeType.NumberOfUpgrades && !_alreadyTaken.Contains((int)upgrade.Type))
                    _alreadyTaken.Add((int)upgrade.Type);
            }
            
            foreach (var upgrade in _addedDroneUpgrades)
                if (_previousMappedDroneUpgrades.ContainsKey(upgrade.Name))
                {
                    var id = _previousMappedDroneUpgrades[upgrade.Name]; //get the previously registered id
                    upgrade.Register(id); //tells the upgrade to create its definition
                    _mappedDroneUpgrades[id] = upgrade; //map it for id > upgrade for upgrade factory
                }
                else
                {
                    var id = FindId(); //get a new id
                    //save it for reusing the next game restart so upgrades can be loaded from game save file
                    _previousMappedDroneUpgrades[upgrade.Name] = id;
                    upgrade.Register(id); //tells the upgrade to create its definition
                    _mappedDroneUpgrades[id] = upgrade; //map it for id>upgrade for upgrade factory
                }

            _previousMappedDroneUpgrades["nextId"] = _nextId; //save the next available id 
            //Save it into a file
            string path = Path.Combine(GameFileHelper.GetBaseGameFileLocation(), "DSMF-droneUpgradeData.xml");
            Plugin.LOGGER.LogMessage("Saving to " + path);
            XmlHelper.ObjectToFile(_previousMappedDroneUpgrades, path);
        }

        private int FindId()
        {
            while (true)
            {
                var id = _nextId++;
                if (!_alreadyTaken.Contains(id))
                    return id;
            }
        }
        public IEnumerable<ModDroneUpgradeContainer> GetAllModUpgrades()
        {
            return _mappedDroneUpgrades.Values.ToList();
        }

        internal void LoadMapping() {
            var mapped = XmlHelper.FileToObject<Dictionary<string, int>>(
            Path.Combine(GameFileHelper.GetBaseGameFileLocation(), "DSMF-droneUpgradeData.xml"));
            if (mapped == null) return;
            _previousMappedDroneUpgrades = mapped;
            _nextId = mapped["nextId"];
        }
    }
    
  

    /// <summary>
    ///     Modded drone upgrades must be wrapped for data persistence, this class helps with the hard part of it
    /// </summary>
    public abstract class ModDroneUpgradeContainer
    {
        public readonly int Cost;
        public readonly string Description;
        public readonly int Duration;
        public readonly int Modifier;
        public readonly string Name;
        public readonly DroneUpgradeClass UpgradeClass;
        protected DroneUpgradeDefinition MyDefinition;

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
            Name = name;
            Description = description;
            Cost = cost;
            Modifier = modifier;
            Duration = duration;
            UpgradeClass = upgradeClass;
        }

        /// <summary>
        ///     Creates a new container with the following properties
        /// </summary>
        /// <param name="name">The name of this upgrade</param>
        /// <param name="description">The description of this upgrade</param>
        /// <param name="upgradeClass">The class the upgrade belongs to</param>
        protected ModDroneUpgradeContainer(string name, string description, DroneUpgradeClass upgradeClass)
        {
            Name = name;
            Description = description;
            UpgradeClass = upgradeClass;
            Cost = 3;
            Modifier = 0;
            Duration = 0;
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
            MyDefinition = new DroneUpgradeDefinition(id.ToString(),
                "true",
                Name,
                Description,
                "0",
                "0",
                Cost.ToString(),
                Modifier.ToString(),
                "0",
                Duration.ToString(),
                UpgradeClass.ToString());
        }

        /// <summary>
        ///     Creates a new instance of a modded drone upgrade, must be implemented and use the internal definition
        /// </summary>
        /// <returns>Must returns a new modded drone upgrade using the registered definition</returns>
        public abstract BaseDroneUpgrade MakeUpgrade();

        public abstract List<CommandDefinition> GetCommandDefinitions();
        /// <summary>
        ///     The definition of this drone upgrade, may be null if called before the game is fully loaded
        /// </summary>
        /// <returns>The definition of this upgrade</returns>
        public DroneUpgradeDefinition GetDefinition()
        {
            return MyDefinition;
        }
        
    }
}