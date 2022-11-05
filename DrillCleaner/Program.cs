using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript {
    partial class Program : MyGridProgram {
        List<IMyShipDrill> drills = new List<IMyShipDrill>();
        List<IMyCargoContainer> containers = new List<IMyCargoContainer>();
        public Program() {
            GridTerminalSystem.GetBlocksOfType(drills);
            GridTerminalSystem.GetBlocksOfType(containers);

            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main(string argument, UpdateType updateSource) {
            // Iterate over each drill, and move its items to a container
            foreach (var drill in drills) {
                // Iterate backwards over the drill's inventory
                for (int i = drill.GetInventory().ItemCount - 1; i >= 0; i--) {
                    // j is container index
                    int j = 0;
                    bool foundAvailableContainer = false;
                    // Iterate over all containers
                    for (; j < containers.Count; j++) {
                        // If the container can store the items from the drill
                        if (containers[j].GetInventory().CanItemsBeAdded(drill.GetInventory().GetItemAt(i).Value.Amount, drill.GetInventory().GetItemAt(i).Value.Type)) {
                            foundAvailableContainer = true;
                            break;
                        }
                    }
                    // Transfer items from drill to available container
                    if (foundAvailableContainer) {
                        drill.GetInventory().TransferItemTo(containers[j].GetInventory(), drill.GetInventory().GetItemAt(i).Value);
                    }
                }
            }
        }
    }
}
