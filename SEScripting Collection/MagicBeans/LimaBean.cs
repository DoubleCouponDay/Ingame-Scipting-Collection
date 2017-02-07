using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace MagicBeans5
{
    class Program : MyGridProgram
    {
#region in-game
        struct Names
        {
            public const string GridName = "LimaBean ";
            public const long OWNED_BY_NO_ONE = 0; 
            public const string FAST_REFRESH = "TriggerNow";
            public const string SLOW_REFRESH = "Start";
        }

        public Program()
        {

        }

        public void Main()
        {
            if (Me.OwnerId != Names.OWNED_BY_NO_ONE) //set all blueprints to owned by 'Me' for this to work universally.
            {
                Me.CubeGrid.CustomName = Names.GridName + " " + Me.CubeGrid.EntityId; // i have to include entity id so that it has a unique name.
            }
        }

        public void Save()
        {

        }
#endregion in-game
    }    
}

