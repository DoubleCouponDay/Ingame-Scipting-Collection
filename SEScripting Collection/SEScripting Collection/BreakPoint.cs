#region PRE_SCRIPT
using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace Ingame_Scripting_Collection
{
    public class Breakpoint : MyGridProgram
    {
#endregion PRE_SCRIPT


        public Program()
        {

        }

        public void Main()
        {

        }

        void BreakPoint (string data_label, string data)
        {
            IMyTextPanel console = GridTerminalSystem.GetBlockWithName ("debug console") as IMyTextPanel;
            console.WritePublicText (data_label + data, true); 
            console.ShowPublicTextOnScreen ();    
        }

        public void Save()
        {

        }
#region POST_SCRIPT
    }    
}
#endregion POST_SCRIPT



