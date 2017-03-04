
using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using System.Text;

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
            public const string MY_CONSOLE_NAME = "MBNavigator";
            public const string NEW_LINE = "\n";
            public const char COMMAND_SEPARATOR = '_';
            public const char SPACE = ' ';
        }

        static class Errors
        {
            public const string NO_BLOCK = " ERROR! Block(s) not found: ";
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
        Dictionary <Vector3I, List <IMyThrust>> matchedThrusterDirections;

        StringBuilder concatLite = new StringBuilder();

        bool compiled;

        void Initialise()
        {
            int nullCounter = default (int);
            gyroscope = GridTerminalSystem.GetBlockWithName (Names.GYRO) as IMyGyro;
            remoteControl = GridTerminalSystem.GetBlockWithName (Names.REMOTE_CONTROL) as IMyRemoteControl;
            GridTerminalSystem.GetBlocksOfType (allOfTheThrusters);

            matchedThrusterDirections = new Dictionary <Vector3I, List <IMyThrust>>()
            {
                { Vector3I.Up, upThrusters },
                { Vector3I.Down, downThrusters },
                { Vector3I.Left, leftThrusters },
                { Vector3I.Right, rightThrusters },
                { Vector3I.Forward, forwardThrusters },
                { Vector3I.Backward, backThrusters },
            };

            nullCheckCollection = new List <object>()
            {
                gyroscope,
                remoteControl,
            };            

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

                    if (matchedThrusterDirections.ContainsKey (currentDirection))
                    {
                        matchedThrusterDirections[currentDirection].Add (allOfTheThrusters[i]);
                    }

                    else
                    {
                        nullCounter++;
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

            if (procedureList.IsNullOrEmpty() == false &&
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
        
        /// <summary>
        /// Goes to an input location and does not expect it to have a length or direction.  
        /// </summary>
        /// <param name="inputLocation"></param>
        void GoToLocation (Vector3D inputLocation)
        {
            Vector3D dronesPosition = Me.CubeGrid.GetPosition();
            double twoVectorDotProduct = (dronesPosition.X * inputLocation.X) + (dronesPosition.Y * inputLocation.Y) + (dronesPosition.Z * dronesPosition.Z);

            if (dronesPosition.Length() != 0.0F)
            {
                double angleBetween = Math.Acos (twoVectorDotProduct / (dronesPosition.Length() * inputLocation.Length()));
                MyBlockOrientation. remoteControl.Orientation.
            }                        
                //to do: pythagorean theorem in 3D to find distance to location
                //to do: somehow get grids rotation relative to universes center 
                //to do: then compare it with angle between two vectors. 
        }

        public void Save()
        {

        } 
#endregion in-game
    }    
}
