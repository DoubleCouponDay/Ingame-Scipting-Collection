﻿
using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace MagicBeans15
{
    class Program : MyGridProgram
    {
#region in-game
        struct Names
        {
            public const string GridName = "KidneyBean ";
            public const long NO_OWNER = 0; 
            public const string FAST_REFRESH = "TriggerNow";
            public const string SLOW_REFRESH = "Start";
        }

        public Program()
        {

        }

        public void Main()
        {
            if (Me.OwnerId != Names.NO_OWNER) //set all blueprints to owned by 'Me' for this to work universally.
            {
                Me.CubeGrid.CustomName = Names.GridName + " " + Me.CubeGrid.EntityId;
            }
        }

        public void Save()
        {

        }
    #endregion in-game
    }
}
