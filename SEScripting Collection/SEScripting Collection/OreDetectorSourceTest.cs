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
            timer = GridTerminalSystem.GetBlockWithName ("Timer Block") as IMyTimerBlock;
            oreDetector = GridTerminalSystem.GetBlockWithName ("Ore Detector") as IMyOreDetector;
            debugConsole = GridTerminalSystem.GetBlockWithName ("LCD Panel") as IMyTextPanel;
            oreMarkers = new List <MyOreMarker>();
        }

        public void Main()
        { 
            oreDetector.GetOreMarkers (ref oreMarkers);            
            debugConsole.WritePublicText ("oremarkers: " + oreMarkers.Count.ToString() + "\n", false);

            for (int i = 0; i < oreMarkers.Count; i++)
            {   
                Vector3D difference = oreDetector.GetPosition() - oreMarkers[i].Location;
                double absoluteDistance = Math.Round (Math.Sqrt (difference.LengthSquared()), 3);
                
                debugConsole.WritePublicText (oreMarkers[i].ElementName + ", " + 
                                              "distance: " + absoluteDistance.ToString() + "\n"
                                              , true);
            }                 
            debugConsole.ShowPublicTextOnScreen();
            Save();
        }
        
        public void Save()
        {
            timer.ApplyAction ("TriggerNow");
        }
#region POST_SCRIPT
    }    
}
#endregion POST_SCRIPT