﻿using Sandbox.Game.EntityComponents;
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
        const string LCD_NAME = "LCD [Power]";

        const int ITERS_PER_SEC = 6;
        const int ITERS_AVG = ITERS_PER_SEC * 60 * 5;
        List<int> prev_averages;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            pa = new List<>(ITERS_PER_SEC);
        }


        static string GetLCDSecondsStr(int seconds)
        {
            return TimeSpan.FromSeconds(seconds).ToString(@"hh\:mm\:ss") + " remaining";
        }
      
        public void Main(string argument, UpdateType updateSource)
        {
            long my_grid = Me.CubeGrid.EntityId;

            var batts = new List<IMyBatteryBlock>();
            // Filter out only batteries on our grid
            GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batts, bat => bat.CubeGrid.EntityId == my_grid);

            float AvgSp = batts.Sum(bat => bat.CurrentStoredPower) / batts.Count;
            float AvgMsp = batts.Sum(bat => bat.MaxStoredPower) / batts.Count;
            float AvgOp = batts.Sum(bat => bat.CurrentOutput) / batts.Count;

            int pb; //Power Bar
            int pn; //Power Now
            int pa; //Power Avg

            float charge_ratio = AvgSp / AvgMsp;
            pb = (int) (charge_ratio * 10); // it's good to round down here.

            float hours_left = AvgSp / AvgOp;
            pn = (int) (hours_left * 60 * 60); // convert to seconds

            if(pa.Count >= ITERS_AVG)
            {
                pa.RemoveAt(0);
            }
            pa.Add(pn);

            pa = prev_averages.Sum() / pa.Count;

            /* Write to LCD */
            IMyTextSurface txt = (IMyTextSurface) GridTerminalSystem.GetBlockWithName(LCD_NAME);

            var buf = new StringBuilder();

            int a = 1;
            while(a < 10)
            {
                buf.Append('|');
                if(pb >= a) {
                    buf.Append("█");
                } else {
                    buf.Append(' ');
                }

                ++a;
            }
            buf.AppendLine('|');

            buf.AppendLine(GetLCDSecondsStr(pn));
            buf.Append(GetLCDSecondsStr(pa)).AppendLine(" (5m)");

            txt.WriteText(buf.ToString());
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