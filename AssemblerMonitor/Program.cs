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
            public MyFixedPoint amount;
            public DateTime lastWorked;

            public QueuedItem(MyFixedPoint amount, DateTime lastWorked) {
                this.amount = amount;
                this.lastWorked = lastWorked;
            }

            public bool IsActive(DateTime currentTick) {
                return (currentTick - lastWorked).TotalSeconds <= 10.0;
            }
        }

        List<IMyAssembler> assemblers;
        List<IMyTextPanel> panels;

        Dictionary<MyDefinitionId, QueuedItem> currAssemblerQueue, prevAssemblerQueue;

        public Program() {
            assemblers = new List<IMyAssembler>();
            GridTerminalSystem.GetBlocksOfType(assemblers);
            if (assemblers.Count == 0) {
                throw new Exception("No assemblers found on grid.");
            }

            panels = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType(panels, panel => panel.CustomName.Contains("[Assembler]"));
            if (panels.Count == 0) {
                throw new Exception("No LCDs found on grid. Tag an LCD with [Assembler] to use with this script.");
            }

            currAssemblerQueue = new Dictionary<MyDefinitionId, QueuedItem>();
            prevAssemblerQueue = new Dictionary<MyDefinitionId, QueuedItem>();

            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main(string argument, UpdateType updateSource) {
            // Runs every 10 ticks

            // Go through each assembler and add their queues to a master list
            currAssemblerQueue.Clear();
            List<MyProductionItem> items = new List<MyProductionItem>();
            for (int i = 0; i < assemblers.Count; ++i) {
                items.Clear();
                assemblers[i].GetQueue(items);
                if (assemblers[i].Mode == MyAssemblerMode.Assembly) {
                    for (int j = 0; j < items.Count; ++j) {
                        
                        // Item already in dict, just add to the amount
                        if (currAssemblerQueue.ContainsKey(items[j].BlueprintId)) {
                            currAssemblerQueue[items[j].BlueprintId].amount += items[j].Amount;
                        } else {
                            // Item not in dict, create new QueuedItem and add it.
                            // TODO: Add timestamp to newly created QueuedItem, for future pruning
                            if (prevAssemblerQueue.ContainsKey(items[j].BlueprintId)) {
                                currAssemblerQueue.Add(items[j].BlueprintId, new QueuedItem(items[j].Amount, prevAssemblerQueue[items[j].BlueprintId].lastWorked));
                            } else {
                                currAssemblerQueue.Add(items[j].BlueprintId, new QueuedItem(items[j].Amount, DateTime.MinValue));
                            }
                        }
                    }
                }
            }

            // Compare master list to previous runs list and check for active items
            if (prevAssemblerQueue.Count > 0) {
                foreach (var item in currAssemblerQueue) {
                    // If item in current list is present in prev list
                    if (prevAssemblerQueue.ContainsKey(item.Key)) {
                        // If amount decreased mark as active
                        if (item.Value.amount < prevAssemblerQueue[item.Key].amount) {
                            item.Value.lastWorked = DateTime.UtcNow;
                        }
                    } else {
                        // New item queued, doesn't necessarily mean it's active though
                    }
                }
            }

            // Replace prev list with the master list
            prevAssemblerQueue.Clear();
            foreach (var item in currAssemblerQueue) {
                prevAssemblerQueue.Add(item.Key, item.Value);
            }

            // Sort the dict
            List<KeyValuePair<MyDefinitionId, QueuedItem>> myList = currAssemblerQueue.ToList();
            myList.Sort(
                delegate(KeyValuePair<MyDefinitionId, QueuedItem> item1, KeyValuePair<MyDefinitionId, QueuedItem> item2) {
                    if (item1.Value.IsActive(DateTime.UtcNow)) {
                        if (item2.Value.IsActive(DateTime.UtcNow)) {
                            return item1.Value.amount.ToIntSafe().CompareTo(item2.Value.amount.ToIntSafe());
                        } else return -1;
                    }
                    else return item1.Value.amount.ToIntSafe().CompareTo(item2.Value.amount.ToIntSafe());
                }
            );


            foreach (var panel in panels) {
                panel.ContentType = ContentType.SCRIPT;
                panel.Script = "";

                var frame = panel.DrawFrame();

                RectangleF viewport = new RectangleF((panel.TextureSize - panel.SurfaceSize) / 2f, panel.SurfaceSize);

                DrawFrame(ref frame, viewport, myList);

                frame.Dispose();


                continue;
                panel.WriteText("Items in Queue\n");
                if (currAssemblerQueue.Count > 0) {
                    panel.WriteText("Assembling:\n", true);
                    foreach (var item in currAssemblerQueue) {
                        panel.WriteText(string.Format("{0,-15} | {1} {2} {3}\n", GetSanitizedName(item.Key.SubtypeName), item.Value.amount.ToIntSafe(), item.Value.IsActive(DateTime.UtcNow) ? "T" : "F", item.Value.lastWorked), true);
                    }
                }
            }
        }

        void DrawFrame(ref MySpriteDrawFrame frame, RectangleF viewport, List<KeyValuePair<MyDefinitionId, QueuedItem>> items) {
            var pos = new Vector2(5, 0) + viewport.Position;

            var sprite = new MySprite() {
                Type = SpriteType.TEXT,
                Data = "Assembling:",
                Position = pos,
                RotationOrScale = 1.5f,
                Color = Color.White,
                Alignment = TextAlignment.LEFT,
                FontId = "White"
            };
            frame.Add(sprite);
            pos += new Vector2(0, 40);


            foreach (var item in items) {
                //panel.WriteText(string.Format("{0,-15} | {1} {2} {3}\n", GetSanitizedName(item.Key.SubtypeName), item.Value.amount.ToIntSafe(), item.Value.IsActive(DateTime.UtcNow) ? "T" : "F", item.Value.lastWorked), true);
                sprite = new MySprite() {
                    Type = SpriteType.TEXT,
                    Data = string.Format("{0,-15} | {1}\n", GetSanitizedName(item.Key.SubtypeName), item.Value.amount.ToIntSafe(), item.Value.lastWorked),
                    Position = pos,
                    RotationOrScale = 1.0f,
                    Color = item.Value.IsActive(DateTime.UtcNow) ? Color.Green : Color.Red,
                    Alignment = TextAlignment.LEFT,
                    FontId = "White"
                };
                frame.Add(sprite);

                pos += new Vector2(0, 22);
            }
        }

        string GetSanitizedName(string name) {
            return name
                .Replace("Component", "")
                .Replace("BP", "")
                .Replace("Communication", "");
        }
    }
}
