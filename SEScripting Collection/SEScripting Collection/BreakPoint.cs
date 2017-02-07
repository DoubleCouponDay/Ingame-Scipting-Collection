
using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace Ingame_Scripting_Collection5
{
    public class Program : MyGridProgram
    {
#region in-game


        public Program()
        {

        }

        public void Main()
        {

        }

        void BreakPoint (string data_label, string data)
        {
            IMyTextPanel console = GridTerminalSystem.GetBlockWithName ("debug console") as IMyTextPanel;
            console.WritePublicText (data_label + data + "\n", true); 
            console.ShowPublicTextOnScreen ();    
        }

        public void Save()
        {

        }
#endregion in-game
    }
}




