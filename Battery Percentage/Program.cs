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

namespace IngameScript
{
    partial class Program : MyGridProgram
    {

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
        }
      
        public void Main(string argument, UpdateType updateSource)
        {
            var batts = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batts);
            var mybatts = new List<IMyBatteryBlock>();
            
            // Filter out only batteries on our grid
            foreach (IMyBatteryBlock batt in batts)
            {
                long bg = batt.CubeGrid.EntityId; // Battery on Grid of programmable block
                long cg = Me.CubeGrid.EntityId;   // Grid on the programmable block
                if (cg == bg)
                {
                    mybatts.Add(batt);
                }
            }

            /* Calcaulate sum of values */
            float SumSp = 0;   //Sum of sp
            float SumMsp = 0;  //average of msp
            float Sumop = 0;   //average of op
            foreach (IMyBatteryBlock mybatt in mybatts)
            {
                float Sp;  //Stored Power
                float Msp; //Max Stored Power
                float Op;  // Output
                Sp = mybatt.CurrentStoredPower;
                Msp = mybatt.MaxStoredPower;
                Op = mybatt.CurrentOutput;

                SumSp = SumSp + Sp;
                SumMsp = SumMsp + Msp;
                Sumop = Sumop + Op;  
            }
            // Take averages of sums
            float AvgSp = SumSp / mybatts.Count;   // Stored Power in MWh
            float AvgMsp = SumMsp / mybatts.Count; // Max stored power in MWh
            float AvgOp = Sumop / mybatts.Count;   // Output in MW



            int pb = (int)(AvgSp / AvgMsp * 10); // Power in Seconds
            Echo(pb.ToString());



            int pn = (int)(AvgSp / AvgOp * 60 * 60);
            string seconds = TimeSpan.FromSeconds(pn).ToString(@"hh\:mm\:ss") + "remaining";
            Echo(seconds);

        }
    }

}
/*    -- LCD --
 * stored, max stored, output
                                Stored power represented as |█| (mulitples of 10%)
 *                              Stored Power: |█|█|█|█|█|█|█|█|█|█|
 *                              1h (now)
 *                              30m (past 5m of  usage)
 */


/*    -- Planning Lvl 1 --
 *    
 *    We want write to the LCD based on the values of some types.
 *                              Stored Power: int (value 0-10)
 *                              time: int (seconds of power) 
 *                              time (5m): int (seconds of power)
 */
