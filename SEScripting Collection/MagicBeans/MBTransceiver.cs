#region PRE_SCRIPT
using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using VRage.Game;

namespace MagicBeans
{
    class MBTransceiver : MyGridProgram
    {
#endregion PRE_SCRIPT
        struct Names
        {
            public const string SCREEN = "DIAGNOSTICS SCREEN";
            public const string ANTENNA = "RADIO ANTENNA";    
            public const string NAV_SCRIPT_NAME = "PBNavigation";      
        }

        struct Commands
        {

            
        }

        struct Messages
        {
            public const string NULL_ERROR = "TRANSCEIVER ERROR: block is null ";
        } 

        string serialMemory;
        static IMyRadioAntenna antenna;

        readonly object[] nullCheckCollection = {
            antenna,
        };

        public Program()
        {
            antenna = GridTerminalSystem.GetBlockWithName (Names.ANTENNA) as IMyRadioAntenna;

            for (int i = 0; i < nullCheckCollection.Length; i++)
            {
                if (nullCheckCollection[i] == null)
                {
                    Echo (Messages.NULL_ERROR);
                }
            }
        }

        public void Main (string input)
        {

        }

        public void Save()
        {                       
        }
#region POST_SCRIPT
    }    
}
#endregion POST_SCRIPT
