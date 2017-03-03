
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
            public const string REMOTE_CONTROL = "REMOTE CONTROL";
            public const string MY_CONSOLE_NAME = "MBTransceiver: ";
            public const string NEW_LINE = "\n";
            public const char COMMAND_SEPARATOR = '_';
            public const char SPACE = ' ';
            public const string Y_CONVENTION = "Y:";
            public const string Z_CONVENTION = "Z:";
        }

        static class Errors
        {
            public const string NO_BLOCK = "ERROR! Block(s) not found: ";
        }

        IMyRemoteControl remoteControl;
        IMyGyro gyroscope;
        List <object> nullCheckCollection;

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
            remoteControl = GridTerminalSystem.GetBlockWithName (Names.REMOTE_CONTROL) as IMyRemoteControl;
            nullCheckCollection = new List <object>()
            {
                gyroscope,
                remoteControl,
            };
            GridTerminalSystem.GetBlocksOfType (allOfTheThrusters);

            for (int i = 0; i < nullCheckCollection.Count; i++) 
            {
                if (nullCheckCollection[i] == null)
                {
                    nullCounter++;
                }
            }

            if (remoteControl != null) //need to check remote exists before comparing with thrusters. pretty sure this dictates the forward direction.
            {
                for (int i = 0; i < allOfTheThrusters.Count; i++)
                {
                    Vector3I currentDirection = allOfTheThrusters[i].GridThrustDirection; 
                    
                    if (currentDirection == Vector3I.Up)
                    {
                        upThrusters.Add (allOfTheThrusters[i]);
                    }

                    else if (currentDirection == Vector3I.Down)
                    {
                        downThrusters.Add (allOfTheThrusters[i]);
                    }

                    else if (currentDirection == Vector3I.Left)
                    {
                        leftThrusters.Add (allOfTheThrusters[i]);
                    }

                    else if (currentDirection == Vector3I.Right)
                    {
                        rightThrusters.Add (allOfTheThrusters[i]);
                    }

                    else if (currentDirection == Vector3I.Forward)
                    {
                        forwardThrusters.Add (allOfTheThrusters[i]);
                    }

                    else if (currentDirection == Vector3I.Backward)
                    {
                        backThrusters.Add (allOfTheThrusters[i]);
                    }
                }
            }

            if (nullCounter != default (int))
            {
                Echo (Errors.NO_BLOCK + nullCounter.ToString());
            }
        }

        public void Main()
        {
            if (compiled)
            {
                CheckForInternalCommunication();
            }

            else
            {
                Initialise();
            }
        }

        /// <summary>
        ///Checks custom data storage for an internal communication.
        /// </summary>
        void CheckForInternalCommunication()
        {
            //Echo("CheckForInternalCommunication start");
            string[] procedureList = Me.CustomData.Split (Names.COMMAND_SEPARATOR);

            if (procedureList != null &&
                procedureList.Length >= default (int))
            {
                Vector3D possibleVector;

                if (Vector3D.TryParse (procedureList[default (int)], out possibleVector))
                {
                    GoToLocation (possibleVector);
                }
            }
            //Echo("CheckForInternalCommunication end");
        }
        
        void GoToLocation (Vector3D inputLocation)
        {
            Me.
            inputLocation
        }

        public void Save()
        {

        } 
#endregion in-game
    }    
}
