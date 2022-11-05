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
using VRage.Utils;
using VRageMath;

namespace IngameScript {
    partial class Program : MyGridProgram {
        // Get all assemblers on grid++
        // Combine all queues into master queue
        // Evenly divide master queue into all assemblers

        Dictionary<MyDefinitionId, MyFixedPoint> masterQueue = new Dictionary<MyDefinitionId, MyFixedPoint>();
        List<IMyAssembler> assemblers = new List<IMyAssembler>();

        List<Dictionary<MyDefinitionId, MyFixedPoint>> tempAssemblerQueues = new List<Dictionary<MyDefinitionId, MyFixedPoint>>();

        int counter;
        int intervalTicks = 6 * 60 * 2;

        public Program() {
            // Get all assemblers on grid
            GridTerminalSystem.GetBlocksOfType(assemblers);
            
            // Remove assemblers that are disassembling or in coop mode
            for (int i = assemblers.Count - 1; i >= 0; i--) {
                if (assemblers[i].CooperativeMode || assemblers[i].Mode == MyAssemblerMode.Disassembly || assemblers[i].Repeating)
                    assemblers.RemoveAt(i);
            }

            // If no usable assemblers
            if (assemblers.Count == 0)
                throw new Exception("No Assemblers found on grid. Atleast 1 needed to use this script.");

            Runtime.UpdateFrequency = UpdateFrequency.Update10 | UpdateFrequency.Once;
        }

        public void Main(string argument, UpdateType updateSource) {
            if (updateSource == UpdateType.Once) {
                counter = intervalTicks;
            } else counter++;

            if (counter != intervalTicks) return;


            // Go through each assembler and add it's queue to the master queue
            foreach (var assembler in assemblers) {
                tempAssemblerQueues.Add(new Dictionary<MyDefinitionId, MyFixedPoint>());
                List<MyProductionItem> assemblerQueue = new List<MyProductionItem>();
                assembler.GetQueue(assemblerQueue);

                // For each item in the assembler's queue
                foreach (var item in assemblerQueue) {
                    // Add the id and amount to our masterQueue
                    if (masterQueue.ContainsKey(item.BlueprintId)) {
                        masterQueue[item.BlueprintId] += item.Amount;
                    } else {
                        masterQueue.Add(item.BlueprintId, item.Amount);
                    }
                }
            }

            // Copy keys to list to allow modifying the dict while iterating over keys
            List<MyDefinitionId> itemIds = new List<MyDefinitionId>(masterQueue.Keys);
            foreach (var itemId in itemIds) {
                // For each assembler
                for (int i = 0; i < tempAssemblerQueues.Count; i++) {
                    // Take a fraction of the total master item stack off and insert it into the assembler's queue
                    MyFixedPoint amountToConsume = MyFixedPoint.Floor(MyFixedPoint.MultiplySafe(1f / assemblers.Count, masterQueue[itemId]));
                    tempAssemblerQueues[i].Add(itemId, amountToConsume);
                    masterQueue[itemId] = MyFixedPoint.AddSafe(masterQueue[itemId], -amountToConsume);
                }
                // Distribute the remaining items to each queue evenly
                int j = 0;
                while (masterQueue[itemId] > 0) {
                    tempAssemblerQueues[j][itemId] += 1;
                    masterQueue[itemId] -= 1;
                    j++;
                    j %= tempAssemblerQueues.Count;
                }
            }

            // Apply the balanced queues to the actual assemblers
            for (int i = 0; i < assemblers.Count; i++) {
                assemblers[i].ClearQueue();
                // Put Tech components at the end of the queue (They take too much crafting time and I want other components to craft first)
                foreach (var item in tempAssemblerQueues[i].OrderBy(item => {
                    if (item.Key.SubtypeName.Contains("Tech")) return 1;
                    return -1;
                })) {
                    if (item.Value > 0) {
                        assemblers[i].AddQueueItem(item.Key, item.Value);
                    }
                }
            }

            masterQueue.Clear();
            tempAssemblerQueues.Clear();
            counter = 0;
        }
    }
}
