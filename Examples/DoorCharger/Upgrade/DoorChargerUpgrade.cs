using System;
using System.Collections.Generic;
using System.Linq;
using ModLoader.Modding;
using UnityEngine;

namespace DoorCharger.Upgrade
{
    public class DoorChargerUpgrade : BaseDroneUpgrade, IStorageUpgrade
    {
        private static List<CommandDefinition> _commandList;
        
        public int Capacity => 4;

        public int Quantity { get; private set; }
        public DoorChargerUpgrade(DroneUpgradeDefinition definition) : base(definition)
        {
            Quantity = Capacity;
        }
        
        public override string CommandValue => "chargedoor";

        public override List<CommandDefinition> QueryAvailableCommands()
        {
            if (_commandList == null)
            {
                _commandList = new List<CommandDefinition>();
                _commandList.Add(new CommandDefinition("chargedoor", "Allows your drone to connect to a door and charge it", "chargedoor d14"));
            }
            return _commandList;
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
                                SendConsoleResponseMessage("charges depleted, unable to power door", ConsoleMessageType.Warning);
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

        public void AddItem(int count)
        {
            Quantity = Math.Min(Quantity + count, Capacity);
        }

        public void OverrideQuantity(int qty)
        {
            Quantity = Math.Min(qty, Capacity);
        }
    }

    public class DoorChargerUpgradeContainer : ModDroneUpgradeContainer
    {
        public DoorChargerUpgradeContainer() :
            base("Door Charger",
                "Allows your drone to connect to a door and charge it.",
                4,
                0,
                0,
                DroneUpgradeClass.Exploration)
        {
        }

        public override BaseDroneUpgrade MakeUpgrade()
        {
            return new DoorChargerUpgrade(myDefinition);
        }
    }
    
    public class AddChargeMod : BaseResupplyMod
    {
        public AddChargeMod() => _name = "Add 2 Charges";

        protected override int ResourceIncreaseValue => 2;
        public override string Description => "adds charges";
        public override int MaxAllowed => 2;
        
        public override IModification CopyModification()
        {
            IModification modification = new AddChargeMod();
            modification.SetTarget(_targetUpgrade);
            return modification;
        }
    }
}