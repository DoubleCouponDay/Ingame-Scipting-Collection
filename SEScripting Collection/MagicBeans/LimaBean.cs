#region PRE_SCRIPT
using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace MagicBeans
{
    class LimaBean : MyGridProgram
    {
        #endregion PRE_SCRIPT
#region in-game
        struct Names
        {
            public const string GridName = "LimaBean ";
            public const long OWNED_BY_NO_ONE = 0; 
        }
/*
        public Program()
        {

        }
*/
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
#endregion
        #region POST_SCRIPT
    }    
}
#endregion POST_SCRIPT
