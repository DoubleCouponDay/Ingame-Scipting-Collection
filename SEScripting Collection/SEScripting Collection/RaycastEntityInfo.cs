using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace Ingame_Scripting_Collection10
{
    class Program : MyGridProgram
    {
#region in-game
        IMyCameraBlock testCamera;
        IMyTimerBlock timer;
        List <IMyRadioAntenna> printer = new List <IMyRadioAntenna>();
        
        public void Main()
        {
            testCamera = GridTerminalSystem.GetBlockWithName ("CAMERA") as IMyCameraBlock;
            timer = GridTerminalSystem.GetBlockWithName ("TIMER") as IMyTimerBlock;
            GridTerminalSystem.GetBlocksOfType (printer);
            testCamera.EnableRaycast = true;
            
            if (testCamera.CanScan (100))
            {
                MyDetectedEntityInfo info = testCamera.Raycast (100);
                
                printer[0].CustomName = info.Name + " / " + info.Relationship.ToString() + " / " + info.Position.ToString();
            }
            timer.ApplyAction ("TriggerNow");
        }
#endregion in-game
    }
}
