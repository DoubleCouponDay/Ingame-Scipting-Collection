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
#region in-game
        struct Names
        {
            public const string UNASSIGNED = "UNASSIGNED";
            public const string SCREEN = "DIAGNOSTICS SCREEN";
            public const string ANTENNA = "RADIO ANTENNA";    
            public const string NAV_SCRIPT_NAME = "PBNavigation";      
        }

        struct Messages
        {
            public const string NO_BLOCK = "TRANSCEIVER ERROR: Block not found.";
        } 

        /// <summary>
        ///Each MB command consists of an action, a target type, and a location. 
        ///I only need to know the first two.
        /// </summary>
        static class Commands //just need to remember that static objects in memory are not released until app closure.
        {
            public static readonly string[] Actions = {
                "ATTACK",
                "RENAME",
                "FOLLOW",
                "GOTO",
                "MINE",                                
            };

            struct Template
            {
                public readonly string SelectedAudience;
                public readonly string Action;
                public readonly string Subject;
                public readonly Vector3D? Location;

                public Template (string selectedAudience, string action, string subject)
                {
                    this.SelectedAudience = selectedAudience;
                    this.Action = action;
                    this.Subject = subject;
                    Location = null;
                }

                public Template (string selectedAudience, string action, string subject, Vector3D location)
                {                    
                    this.SelectedAudience = selectedAudience;
                    this.Action = action;
                    this.Subject = subject;
                    this.Location = location;
                }
            }                        
        }

        bool compiled = true;
        string serialMemory;
        static IMyRadioAntenna antenna;

        readonly object[] nullCheckCollection = {
            antenna,
        };
        
        public Program()
        {
            Me.CubeGrid.CustomName = "UNASSIGNED BEAN";
            antenna = GridTerminalSystem.GetBlockWithName (Names.ANTENNA) as IMyRadioAntenna;            

            for (int i = 0; i < nullCheckCollection.Length; i++)
            {
                if (nullCheckCollection[i] == null)
                {
                    Echo (Messages.NO_BLOCK);
                    compiled = false;
                }
            }
        }

        //public static void Main (string serializedCommand)
        public void Main (string serializedCommand)
        {
            if (compiled)
            {
                
            }
        }

        public void Save() //called by game on session close.
        {                       
        }
#endregion
#region POST_SCRIPT
    }    
}
#endregion POST_SCRIPT
