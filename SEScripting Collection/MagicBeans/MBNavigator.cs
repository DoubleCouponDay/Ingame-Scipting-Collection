
using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace MagicBeans2
{
    class Program : MyGridProgram
    {
#region in-game
        enum Resolutions
        {
            ONE = 1,
            TWO = 2,
            THREE = 3,
            FIVE = 5,
            SIX = 6,
            NINE = 9,
            TEN = 10,
            FIFTEEN = 15,
            EIGHTEEN = 18,
            THIRTY = 30,
            FORTY_FIVE = 45,
            NINETY = 90,
        }

        static class Names
        {
            public const string SCREEN = "PRINT CONSOLE";
            public const string GYRO = "GYROSCOPE";
            public const string MY_CONSOLE_NAME = "MBTransceiver: ";
            public const string NEW_LINE = "\n";
            public const char COMMAND_SEPARATOR = '_';
            public const char SPACE = ' ';
            public const string Y_CONVENTION = "Y:";
            public const string Z_CONVENTION = "Z:";
        }

        IMyGyro gyroscope;
        List <IMyThrust> upThrusters;
        List <IMyThrust> downThrusters;
        List <IMyThrust> leftThrusters;
        List <IMyThrust> rightThrusters;
        List <IMyThrust> forwardThrusters;
        List <IMyThrust> backThrusters;
        List <IMyThrust> allOfTheThrusters;
        
        bool compiled;

        void Initialise()
        {
            int nullCounter = default (int);
            gyroscope = GridTerminalSystem.GetBlockWithName (Names.GYRO) as IMyGyro;
            GridTerminalSystem.GetBlocksOfType (allOfTheThrusters);

            for (int i = 0; i < allOfTheThrusters.Count; i++)
            {
                allOfTheThrusters[i].GridThrustDirection.
            }
            IMyLargeGatlingTurret test;
            test.
        }

        public void Main()
        {
            if (compiled)
            {

            }

            else
            {
                Initialise();
            }
        }

        public void Save()
        {

        }

        public class Command
        {
            public readonly bool IsEmpty;

            public const int SCOPES_INDEX = 0;
            public const int AUDIENCES_INDEX = 1;
            public const int ACTION_INDEX = 2;
            public const int SUBJECT_INDEX = 3;
            public const int VECTORS_INDEX = 4;
            public const int LENGTH = 5;

            public readonly string CommunicationScope;
            public readonly string SelectedAudience; //can be entity Id
            public readonly string Action;
            public readonly string Subject; //can be entity Id
            
            public readonly Vector3D? Location;    
            
            public Command()
            {
                IsEmpty = true;
            }                  
            
            public Command (string communicationScope, string selectedAudience, string action, string subject, Vector3D location)
            {   
                IsEmpty = false;            
                this.CommunicationScope = communicationScope;     
                this.SelectedAudience = selectedAudience;
                this.Action = action;
                this.Subject = subject;
                this.Location = location;                
            } 
        }   
#endregion in-game
    }    
}
