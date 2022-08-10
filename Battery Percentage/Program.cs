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
        const int REFRESH_DELAY_SEC = 5;

        List<int> prev_averages;
        int tick_wait;

        // cached
        List<IMyBatteryBlock> batts;
        float AvgMsp;


        void reset()
		{
            tick_wait = REFRESH_DELAY_SEC * ITERS_PER_SEC;
		}

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            pa = new List<>(ITERS_PER_SEC);

            batts = new List<IMyBatteryBlock>();
        }


        void refreshBatteries()
		{
            long my_grid = Me.CubeGrid.EntityId;

            batts.Clear();
            GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batts, bat => bat.CubeGrid.EntityId == my_grid);
		}

        void refreshCache() {
            refreshBatteries();
            AvgMsp = batts.Select(bat => bat.MaxStoredPower).Average();
		}


        static string GetLCDSecondsStr(int seconds)
        {
            return TimeSpan.FromSeconds(seconds).ToString(@"ddd\.hh\:mm\:ss") + " remaining";
        }
      
        public void Main(string argument, UpdateType updateSource)
        {
            if(--tick_wait <= 0) {
                refreshCache();

                reset();
			}

            float AvgSp = batts.Select(bat=> bat.CurrentStoredPower).Average();
            float AvgOp = batts.Select(bat => bat.CurrentOutput).Average();

            int pb; //Power Bar
            int pn; //Power Now
            int pa; //Power Avg

            float charge_ratio = AvgSp / AvgMsp;
            pb = (int) (charge_ratio * 10); // it's good to round down here.

            float hours_left = AvgSp / AvgOp;
            pn = (int) (hours_left * 60 * 60); // convert to seconds

            if(prev_averages.Count >= ITERS_AVG)
            {
                prev_averages.RemoveAt(0);
            }
            prev_averages.Add(pn);

            pa = prev_averages.Average();

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