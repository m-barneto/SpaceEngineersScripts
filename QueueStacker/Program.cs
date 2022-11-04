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
        List<IMyAssembler> assemblers;

        List<MyProductionItem> items;
        Dictionary<MyDefinitionId, MyFixedPoint> queue;
        HashSet<MyDefinitionId> itemSet;

        int ticker;

        public Program() {
            ticker = 0;
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            items = new List<MyProductionItem>();
            itemSet = new HashSet<MyDefinitionId>();
            queue = new Dictionary<MyDefinitionId, MyFixedPoint>();
            assemblers = new List<IMyAssembler>();

            GridTerminalSystem.GetBlocksOfType(assemblers);
            Echo("Startup");
        }

        public void Main(string argument, UpdateType updateSource) {
            ticker++;
            
            if (ticker >= 10) {
                ticker = 0;
                Echo("Ran");
                for (int i = 0; i < assemblers.Count; ++i) {
                    bool isDirty = false;
                    assemblers[i].GetQueue(items);

                    for (int j = 0; j < items.Count; ++j) {
                        if (isDirty || itemSet.Contains(items[j].BlueprintId)) isDirty = true;
                        else itemSet.Add(items[j].BlueprintId);

                        if (!queue.ContainsKey(items[j].BlueprintId)) {
                            queue.Add(items[j].BlueprintId, items[j].Amount);
                        } else {
                            queue[items[j].BlueprintId] += items[j].Amount;
                        }
                    }
                    if (isDirty) {
                        assemblers[i].ClearQueue();
                        foreach (var item in queue) {
                            assemblers[i].AddQueueItem(item.Key, item.Value);
                        }
                    }
                    itemSet.Clear();
                    items.Clear();
                    queue.Clear();
                }
            }
        }
    }
}
