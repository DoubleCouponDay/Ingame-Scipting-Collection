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
            debugConsole.WritePublicText ("oremarkers: " + oreMarkers.Count.ToString() + "\n", false);
            debugConsole.WritePublicText ("detection range: " + oreDetector.Range.ToString() + "\n", true);

            for (int i = 0; i < oreMarkers.Count; i++)
            {
                double deltaX = oreDetector.GetPosition().X - oreMarkers[i].Location.X;
                double deltaY = oreDetector.GetPosition().Y - oreMarkers[i].Location.Y;
                double deltaZ = oreDetector.GetPosition().Z - oreMarkers[i].Location.Z;
                double distance = Math.Sqrt (Math.Pow (deltaX, 2) + Math.Pow(deltaY, 2) + Math.Pow(deltaZ, 2));
                
                debugConsole.WritePublicText (oreMarkers[i].ElementName + ", " + 
                                              "distance: " + distance.ToString() + "\n"
                                              , true);
            }                 
            debugConsole.ShowPublicTextOnScreen();
            Save();
        }
        
        public void Save()
        {
            timer.ApplyAction ("Start");
        }
#region POST_SCRIPT
    }    
}
#endregion POST_SCRIPT