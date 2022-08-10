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
        const string LCD_NAME = "LCD [Power]";

        const int ITERS_PER_SEC = 6;
        const int ITERS_AVG = ITERS_PER_SEC * 60 * 5;
        const int REFRESH_DELAY_SEC = 5;

        List<int> prev_averages;
        int tick_wait;

        // cached
        IMyTextSurface txt;
        StringBuilder txtBuf;
        List<IMyBatteryBlock> batts;
        float AvgMsp;


        void reset()
		{
            tick_wait = REFRESH_DELAY_SEC * ITERS_PER_SEC;
		}

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            prev_averages = new List<int>(ITERS_PER_SEC);

            txtBuf = new StringBuilder();
            batts = new List<IMyBatteryBlock>();
        }


        void refreshBatteries()
		{
            long my_grid = Me.CubeGrid.EntityId;

            batts.Clear();
            GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batts, bat => bat.CubeGrid.EntityId == my_grid);
		}

        void refreshCache() {
            txt = (IMyTextSurface) GridTerminalSystem.GetBlockWithName(LCD_NAME);

            refreshBatteries();
            AvgMsp = batts.Select(bat => bat.MaxStoredPower).Average();
		}

        struct BatteryTickResult {
            public float AvgSp;  // Stored Power in MWh
            public float AvgMsp; // Max stored power in MWh
            public float AvgOp;  // Output in MW
        }

        BatteryTickResult BatteryTick()
		{
            BatteryTickResult result;

            result.AvgMsp = AvgMsp;
            result.AvgSp = batts.Select(bat=> bat.CurrentStoredPower).Average();
            result.AvgOp = batts.Select(bat => bat.CurrentOutput).Average();

            return result;
		}

        static string GetPowerTime(int seconds)
        {
            return TimeSpan.FromSeconds(seconds).ToString(@"ddd\.hh\:mm\:ss") + " remaining";
        }

        void WriteOut(ref BatteryTickResult br, int power_now, int power_output_5m)
		{
            int pb; //Power Bar

            float charge_ratio = br.AvgSp / br.AvgMsp;
            pb = (int) (charge_ratio * 10); // it's good to round down here.

            /* Write to LCD */
            int a = 1;
            while(a < 10)
            {
                txtBuf.Append('|');
                if(pb >= a) {
                    txtBuf.Append("█");
                } else {
                    txtBuf.Append(" ");
                }

                ++a;
            }
            txtBuf.AppendLine("|");

            txtBuf.AppendLine(GetPowerTime(power_now));
            txtBuf.Append(GetPowerTime(power_output_5m)).AppendLine(" (5m)");

            string txtWr = txtBuf.ToString();
            txtBuf.Clear();
            txt.WriteText(txtWr);
		}

        public void Main(string argument, UpdateType updateSource)
        {
            if(--tick_wait <= 0) {
                refreshCache();

                reset();
			}

            BatteryTickResult br = BatteryTick();

            int power_now;
            float hours_left = br.AvgSp / br.AvgOp;
            power_now = (int) (hours_left * 60 * 60); // convert to seconds

            if(prev_averages.Count >= ITERS_AVG)
            {
                prev_averages.RemoveAt(0);
            }
            prev_averages.Add(power_now);

            WriteOut(ref br, power_now, prev_averages.Sum() / prev_averages.Count);
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