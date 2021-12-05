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
        class QueuedItem {
            public bool active;
            public MyFixedPoint amount;

            public QueuedItem(MyFixedPoint amount) {
                this.amount = amount;
                this.active = false;
            }
        }

        List<IMyAssembler> assemblers;
        List<IMyTextPanel> panels;
        Dictionary<MyDefinitionId, QueuedItem> prodQueue, dissQueue;

        public Program() {
            assemblers = new List<IMyAssembler>();
            GridTerminalSystem.GetBlocksOfType(assemblers);
            if (assemblers.Count == 0) {
                Echo("No assemblers found!");
                return;
            }

            panels = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType(panels, panel => panel.CustomName.Contains("[Assembler]"));
            if (panels.Count == 0) {
                Echo("No LCD's found!");
                return;
            }

            prodQueue = new Dictionary<MyDefinitionId, QueuedItem>();
            dissQueue = new Dictionary<MyDefinitionId, QueuedItem>();

            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main(string argument, UpdateType updateSource) {
            prodQueue.Clear();
            dissQueue.Clear();

            List<MyProductionItem> items = new List<MyProductionItem>();

            for (int i = 0; i < assemblers.Count; ++i) {
                items.Clear();
                assemblers[i].GetQueue(items);
                if (assemblers[i].Mode == MyAssemblerMode.Assembly) {
                    for (int j = 0; j < items.Count; ++j) {
                        // Item already in dict, check if decreased count to mark as active or its count has increased
                        if (prodQueue.ContainsKey(items[j].BlueprintId)) {

                            if (prodQueue[items[j].BlueprintId].amount != items[j].Amount) {
                                // amount changed, if its less than previous, flag as active
                            }
                            prodQueue[items[j].BlueprintId].amount += items[j].Amount;
                        } else {
                            prodQueue.Add(items[j].BlueprintId, new QueuedItem(items[j].Amount));
                        }
                    }
                } else {
                    for (int j = 0; j < items.Count; ++j) {
                        if (dissQueue.ContainsKey(items[j].BlueprintId)) {
                            dissQueue[items[j].BlueprintId].amount += items[j].Amount;
                        } else {
                            dissQueue.Add(items[j].BlueprintId, new QueuedItem(items[j].Amount, items[j].ItemId == assemblers[i].NextItemId));
                        }
                    }
                }
            }


            /*foreach (var panel in panels) {
                continue;
                foreach (var item in prodQueue) {
                    panel.WriteText(string.Format("{0,-15} | {1} {2}\n", GetSanitizedName(item.Key.SubtypeName), item.Value.amount.ToIntSafe(), item.Key.), true);
                }
                panel.WriteText("Items in Queue\n");
                if (prodQueue.Count > 0) {
                    panel.WriteText("Assembling:\n", true);
                    foreach (var item in prodQueue) {
                        panel.WriteText(string.Format("{0,-15} | {1} {2}\n", GetSanitizedName(item.Key.SubtypeName), item.Value.amount.ToIntSafe(), item.Value.active ? "T" : "F"), true);
                    }
                }
                if (dissQueue.Count > 0) {
                    panel.WriteText("Disassembling:\n", true);
                    foreach (var item in dissQueue) {
                        panel.WriteText(string.Format("{0,-15} | {1} {2}\n", GetSanitizedName(item.Key.SubtypeName), item.Value.amount.ToIntSafe(), item.Value.active ? "T" : "F"), true);
                    }
                }
            }*/
        }

        string GetSanitizedName(string name) {
            return name
                .Replace("Component", "")
                .Replace("BP", "")
                .Replace("Communication", "");
        }
    }
}
