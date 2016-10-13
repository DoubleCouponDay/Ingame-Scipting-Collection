#region PRE_SCRIPT
using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace Ingame_Scripting_Collection
{
    public class OreDetectorSourceTest : MyGridProgram
    {
#endregion PRE_SCRIPT
        IMyTimerBlock timer;
        IMyOreDetector oreDetector;
        IMyTextPanel debugConsole;
        List <MyOreMarker> oreMarkers;

        public Program()
        {
            timer = GridTerminalSystem.GetBlockWithName ("timer") as IMyTimerBlock;
            oreDetector = GridTerminalSystem.GetBlockWithName ("ore detector") as IMyOreDetector;
            debugConsole = GridTerminalSystem.GetBlockWithName ("debug console") as IMyTextPanel;
            oreMarkers = new List <MyOreMarker>();
        }

        public void Main()
        {
            oreDetector.GetOreMarkers (ref oreMarkers);
            debugConsole.WritePublicText ("", false);

            foreach (MyOreMarker marker in oreMarkers)
            {
                debugConsole.WritePublicText (marker.ElementName, true);
                debugConsole.WritePublicText (marker.Location.ToString() + @"\n\n", true);
                debugConsole.ShowPublicTextOnScreen();
            }            
        }

        public void Save()
        {
            timer.ApplyAction ("TriggerNow");
        }
#region POST_SCRIPT
    }    
}
#endregion POST_SCRIPT