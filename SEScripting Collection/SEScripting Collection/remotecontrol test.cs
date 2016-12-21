using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace Ingame_Scripting_Collection
{
    class remotecontrol_test : MyGridProgram
    {
        IMyRemoteControl testremote;
        double yeah = 0.0;
        
        public void main()
        {
            testremote = GridTerminalSystem.GetBlockWithName ("REMOTE") as IMyRemoteControl;
            testremote.TryGetPlanetElevation (MyPlanetElevation.Surface, out yeah);
        }
    }
}
