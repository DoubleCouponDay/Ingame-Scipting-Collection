#region PRE_SCRIPT
using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace Ingame_Scripting_Collection
{
    public class TimeToImpact : MyGridProgram
    {
#endregion PRE_SCRIPT
        const string FORWARD_CAM_NAME = "FORWARD CAM";
        const string BOTTOM_CAM_NAME = "BOTTOM CAM";

        static IMyCameraBlock forwardCam;
        static IMyCameraBlock bottomCam;
        IMyTimerBlock timer;

        object[] nullCheckCollection = new object[]
        {
            forwardCam,
            bottomCam,
        };

        bool bootSuccessful = true;

        public Program()
        {
            forwardCam = GridTerminalSystem.GetBlockWithName (FORWARD_CAM_NAME) as IMyCameraBlock;
            bottomCam = GridTerminalSystem.GetBlockWithName (BOTTOM_CAM_NAME) as IMyCameraBlock;

            for (int i = 0; i < nullCheckCollection.Length; i++)
            {
                if (nullCheckCollection[i] == null)
                {
                    bootSuccessful = false;
                }
            }
        }

        public void Main()
        {
            if (bootSuccessful)
            {
                forwardCam.Raycast ();
            }
        }

        public void Save()
        {

        }
#region POST_SCRIPT
    }    
}
#endregion POST_SCRIPT
