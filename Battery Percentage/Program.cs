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

        }
    }
}
/*    -- LCD --
 * stored, output. Stored power represented as |█| (mulitples of 10%)
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