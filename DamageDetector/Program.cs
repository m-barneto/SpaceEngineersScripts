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
        float prevHealth;
        string broadcastChannel = "Mattdokn Receiver";
        IMyRadioAntenna antenna;
        List<IMyLargeGatlingTurret> turrets;
        List<IMyTerminalBlock> blocks;

        public Program() {
            // TPS is 60
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            blocks = new List<IMyTerminalBlock>();
            prevHealth = GetGridHealth();

            var list = new List<IMyRadioAntenna>();
            GridTerminalSystem.GetBlocksOfType(list);
            if (list.Count == 0) {
                throw new Exception("Can't find working antenna!");
            }
            turrets = new List<IMyLargeGatlingTurret>();
            GridTerminalSystem.GetBlocksOfType(turrets, turret => turret.CustomName.Contains("[Targeter]"));

            antenna = list[0];
        }

        public void Main(string argument, UpdateType updateSource) {
            float currHealth = GetGridHealth();
            if (currHealth < prevHealth || turrets.Count > 0 && turrets.Any(turret => turret.HasTarget)) {
                PanicMode();
            }
            prevHealth = currHealth;
        }

        float GetGridHealth() {
            blocks.Clear();
            GridTerminalSystem.GetBlocks(blocks);
            return blocks.Sum(x => x.CubeGrid.GetCubeBlock(x.Position).CurrentDamage);
        }

        void PanicMode() {
            // Get current gps marker
            var pos = Me.GetPosition();
            string gps = $"GPS:{Me.CubeGrid.DisplayName}:{pos.X:n2}:{pos.Y:n2}:{pos.Z:n2}:";
            // Send mayday to all receivers on mayday channel
            IGC.SendBroadcastMessage(broadcastChannel, $"mayday|{gps}");
            // Shut down antenna
            antenna.EnableBroadcasting = false;
            // Proceed to drone home base (hopefully guarded)
        }
    }
}
