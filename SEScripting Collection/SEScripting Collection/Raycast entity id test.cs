﻿using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace Ingame_Scripting_Collection
{
    class RaycastEntityIdTest : MyGridProgram
    {
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
                printer[0].CustomName = info.Name + " / " + info.Relationship.ToString() + " / " + info.EntityId.ToString();
            }
            timer.ApplyAction ("TriggerNow");
        }
    }
}
