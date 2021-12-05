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
        string broadcastChannel = "Mattdokn Receiver";
        IMyBroadcastListener listener;
        List<IMyTextPanel> panels;

        public Program() {
            listener = IGC.RegisterBroadcastListener(broadcastChannel);

            listener.SetMessageCallback(broadcastChannel);

            panels = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType(panels, panel => panel.CustomName.Contains("[Receiver]"));
            if (panels.Count == 0) {
                throw new Exception("Couldn't find panel with name [Receiver]");
            }
            panels.ForEach(panel => { panel.ContentType = ContentType.TEXT_AND_IMAGE; panel.FontColor = Color.Red; });
        }

        public void Main(string argument, UpdateType updateSource) {
            if ((updateSource & UpdateType.IGC) > 0) {
                while (listener.HasPendingMessage) {
                    var msg = listener.AcceptMessage();
                    if (msg.Tag.Equals(broadcastChannel)) {
                        try {
                            string[] data = msg.Data.ToString().Split('|');
                            if (data.Length > 0) {
                                switch (data[0]) {
                                    case "mayday":
                                        Mayday(data);
                                        break;
                                }
                            }
                        } catch (Exception e) {
                            Echo(e.StackTrace);
                        }
                    }
                }
            }
        }

        void Mayday(string[] msg) {
            panels.ForEach(panel => {
                panel.WriteText("Help required at:\n", true);
                panel.WriteText(msg[1] + '\n', true);
            });
        }
    }
}
