
using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using VRage.Game;

namespace MagicBeans3
{
    class Program : MyGridProgram
    {
#region in-game
        struct Names
        {
            public const string UNASSIGNED = "UNASSIGNED";
            public const string SCREEN = "DIAGNOSTICS SCREEN";
            public const string ANTENNA = "RADIO ANTENNA";    
            public const string NAV_SCRIPT_NAME = "PBNavigation";     
            public const long OWNED_BY_NO_ONE = 0; 
            public const string FAST_REFRESH = "TriggerNow";
            public const string SLOW_REFRESH = "Start";
        }

        struct Messages
        {
            public const string NO_BLOCK = "TRANSCEIVER ERROR: Block not found.";
        } 

        static class Commands //just need to remember that static objects in memory are not released until app closure.
        {
            public struct JobActions
            {
                public const string FOLLOW = "FOLLOW";
                public const string MINE = "MINE";    
                public const string BUILD = "BUILD";                          
            }

            public struct PriorityActions
            {
                public const string GOTO = "GOTO";                        
                public const string RENAME = "RENAME";
                public const string STUCK = "STUCK";
                public const string HELP = "HELP";
            }

            public struct Subjects
            {
                public const string BEANSTALK = "BEANSTALK";

                public const string PLATINUM = "PLATINUM";
                public const string GOLD = "GOLD";
                public const string SILVER = "SILVER";
                public const string SILICON = "SILICON";
                public const string NICKEL = "NICKEL";
                public const string URANIUM = "URANIUM";
                public const string MAGNESIUM = "MAGNESIUM";
                public const string IRON = "IRON";
                public const string ICE = "ICE";

                public const string KIDNEYBEAN_AUDIENCE = "KIDNEYBEAN";
                public const string LIMABEAN_AUDIENCE = "LIMABEAN";                
            }

            public struct Template
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

        bool compiled = false;
        string serialMemory;
        static IMyRadioAntenna antenna;

        readonly object[] nullCheckCollection = {
            antenna,
        };
       
        public void Initialise()
        {            
            antenna = GridTerminalSystem.GetBlockWithName (Names.ANTENNA) as IMyRadioAntenna;       
            int nullCount = default (int);     
            
            for (int i = 0; i < nullCheckCollection.Length; i++) 
            {
                if (nullCheckCollection[i] == null)
                {
                    nullCount++;
                    Echo (Messages.NO_BLOCK);
                }
            }
            
            if (nullCount == default (int))
            {
                compiled = true;
            }
        }

        //public static void Main (string serializedCommand)
        public void Main (string serializedCommand)
        {
            if (compiled == false)
            {

            }

            else
            {
                Initialise();
            }
        }

        public void Save() //called by game on session close.
        {                       
        }
#endregion in-game
    }    
}

