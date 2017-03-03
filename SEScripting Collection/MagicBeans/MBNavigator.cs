
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
                    currentDirection.                    
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
                CheckForMoveInstructions();
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
                string firstItem = procedureList[default (int)];
                Command possibleCommand = TryCreateCommand (firstItem);

                if (possibleCommand.IsEmpty == false &&
                    possibleCommand.CommunicationScope == CommunicationModel.CommunicationScopes.INTERNAL)
                {
                    ApplyCommand(possibleCommand);
                }
            }
            //Echo("CheckForInternalCommunication end");
        }

        /// <summary>
        /// Check the IsEmpty property to see if the returned Command is genuine.
        /// Returns Command with everything zeroed if failed.
        /// </summary>
        /// <param name="serialisedCommand"></param>
        /// <param name="possibleSuccessState"></param>
        Command TryCreateCommand(string serialisedCommand)
        {
            //Echo("TryCreateCommand start");
            Command possibleSuccessState = new Command();

            if (serialisedCommand != null)
            {
                string[] sectionedString = serialisedCommand.Split(Names.SPACE);

                if (sectionedString.Length == Command.LENGTH)
                {
                    int letterYPlace = sectionedString[Command.VECTORS_INDEX].IndexOf (Names.Y);
                    sectionedString[Command.VECTORS_INDEX].Insert (letterYPlace, Names.SPACE.ToString());
                    int letterZPlace = sectionedString[Command.VECTORS_INDEX].IndexOf (Names.Z); //since inserting changes the position of all letters, im going to find the next index after Insert()
                    sectionedString[Command.VECTORS_INDEX].Insert (letterZPlace, Names.SPACE.ToString());

                    Vector3D possibleVector;

                    if (Vector3D.TryParse (sectionedString[Command.VECTORS_INDEX], out possibleVector))
                    {
                        possibleSuccessState = new Command (sectionedString[Command.SCOPES_INDEX],
                                                            sectionedString[Command.AUDIENCES_INDEX],
                                                            sectionedString[Command.ACTION_INDEX],
                                                            sectionedString[Command.SUBJECT_INDEX],
                                                            possibleVector
                                                            );
                    }
                }
            }
            //Echo("TryCreateCommand end");
            return possibleSuccessState;
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
