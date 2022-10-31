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
        List<string> tags = new List<string>();
        MyCommandLine cmd = new MyCommandLine();

        public Program() {}

        public void Save() {

        }

        public void Main(string argument, UpdateType updateSource) {
            if (cmd.TryParse(argument)) {
                if (cmd.Switch("save")) {
                    // Create gps point at raycast
                    // Get current gps marker
                    var pos = Me.GetPosition();
                    string gps = $"GPS:{Me.CubeGrid.DisplayName}:{pos.X:n2}:{pos.Y:n2}:{pos.Z:n2}:";
                }
                if (cmd.Switch("addtag")) {
                    // arg 0
                    // Tag Name (Ag, Pl, Fe) etc
                    string tag = cmd.Argument(0);
                    if (tags.Contains(tag)) {
                        tags.Remove(tag);
                    } else {
                        tags.Add(tag);
                    }
                    // Refresh tags list display lcd?
                }
            }
        }
    }
}
