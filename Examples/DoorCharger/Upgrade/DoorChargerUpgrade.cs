using System;
using System.Collections.Generic;
using System.Linq;
using DSMFramework.Modding;
using UnityEngine;

namespace DoorCharger.Upgrade
{
    /// <summary>
    ///     Main upgrade logic, this is what handles all upgrade commands and uses
    /// </summary>
    public class DoorChargerUpgrade : BaseDroneUpgrade, IStorageUpgrade
    {
        private readonly ModDroneUpgradeContainer _myContainer;

        public DoorChargerUpgrade(DroneUpgradeDefinition definition, ModDroneUpgradeContainer myContainer) : base(definition)
        {
            _myContainer = myContainer;
            Quantity = Capacity;
        }

        public override string CommandValue => "chargedoor";

        public int Capacity => 4;

        public int Quantity { get; private set; }

        public void AddItem(int count)
        {
            Quantity = Math.Min(Quantity + count, Capacity);
        }

        public void OverrideQuantity(int qty)
        {
            Quantity = Math.Min(qty, Capacity);
        }

        public override List<CommandDefinition> QueryAvailableCommands()
        {
            return _myContainer.GetCommandDefinitions();
        }

        public override void ExecuteCommand(ExecutedCommand command, bool partOfMultiCommand)
        {
            var commandName = command.Command.CommandName;
            if (commandName != CommandValue)
                return;
            command.Handled = true;

            if (command.Arguments.Count == 0 ||
                command.Arguments.Count != 2 && command.Arguments[0].ToLower() == "all" ||
                command.Arguments.Count != 1 && command.Arguments[0].ToLower() != "all")
            {
                SendConsoleResponseMessage("invalid command.  Ex: remotedoor 1 d23", ConsoleMessageType.Warning);
            }
            else
            {
                var instance = DungeonManager.Instance;
                Door door1 = null;
                var lower = command.Arguments.Last().ToLower();

                foreach (var door2 in instance.doors)
                    if (door2.LabelSimple.ToLower() == lower)
                    {
                        door1 = door2;
                        break;
                    }

                if (door1 != null && door1.corridor.containsRoom(drone.CurrentRoom))
                {
                    var bounds = door1.corridor.GetComponent<Collider>().bounds;
                    bounds.Expand(new Vector3(0.3f, 0.3f, 0.3f));
                    if (bounds.Intersects(drone.GetComponent<Collider>().bounds))
                    {
                        if (door1.powered)
                        {
                            SendConsoleResponseMessage("door already powered: " + lower, ConsoleMessageType.Info);
                        }
                        else
                        {
                            if (Quantity <= 0 || !UpgradeUsed())
                            {
                                SendConsoleResponseMessage("charges depleted, unable to power door",
                                    ConsoleMessageType.Warning);
                                return;
                            }

                            Quantity--;
                            SendConsoleResponseMessage("powered door: " + lower, ConsoleMessageType.Benefit);
                            door1.power(true);
                            DroneManager.Instance.currentDronePanel.UpgradesChanged = true;
                            
                        }
                    }
                    else
                    {
                        drone.NavigateToAndExecuteCommand(door1.corridor.gameObject, command,
                            CollisionType.BoundsIntesect);
                    }
                }
                else
                {
                    SendConsoleResponseMessage("specified door not found: " + lower, ConsoleMessageType.Warning);
                }
            }
        }
    }

    /// <summary>
    ///     The container for our upgrade, allowing it to be registered in the game
    /// </summary>
    public class DoorChargerUpgradeContainer : ModDroneUpgradeContainer
    {
        private List<CommandDefinition> commandList =  new List<CommandDefinition>();
        public DoorChargerUpgradeContainer() :
            base("Door Charger",
                "Allows your drone to connect to a door and charge it.",
                4,
                0,
                0,
                DroneUpgradeClass.Exploration)
        {
            commandList.Add(new CommandDefinition("chargedoor",
                "Allows your drone to connect to a door and charge it", "chargedoor d14"));
        }

        public override BaseDroneUpgrade MakeUpgrade()
        {
            //Example all implementations should follow for creating a new upgrade instance
            return new DoorChargerUpgrade(MyDefinition, this);
        }

        public override List<CommandDefinition> GetCommandDefinitions()
        {
            return commandList;
        }
    }

    /// <summary>
    ///     The modification for the modification menu to allow player to recharge the upgrade
    /// </summary>
    public class AddChargeMod : BaseResupplyMod
    {
        public AddChargeMod()
        {
            _name = "Add 2 Charges"; //Label of the modification in the menu
        }

        protected override int ResourceIncreaseValue => 2; //How much it will increase when brought

        public override string Description =>
            "adds charges"; //A small description that will appear at the bottom of the menu

        public override int MaxAllowed => 2; //No idea what it does, but setting it to 2 seems to work

        public override IModification CopyModification()
        {
            //Seems to like to create a new instance of this modification, just do like other modifications
            IModification modification = new AddChargeMod();
            modification.SetTarget(_targetUpgrade);
            return modification;
        }
    }
}