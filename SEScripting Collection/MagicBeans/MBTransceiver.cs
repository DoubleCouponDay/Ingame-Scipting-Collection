
using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using VRage.Game;
using System.Collections.ObjectModel;
using System.Text;

namespace MagicBeans3
{
    class Program : MyGridProgram
    {
#region in-game
        static class Names
        {
            public const string SCREEN = "DIAGNOSTICS SCREEN";
            public const string ANTENNA = "RADIO ANTENNA";              
            public const string MY_CONSOLE_NAME = "MBTransceiver: ";   
            public const string NEW_LINE = "\n";
            public const char SPACE = ' ';           
        }

        struct Messages
        {
            public const string NO_BLOCK = "TRANSCEIVER ERROR: Block not found; ";
        } 

        bool compiled;

        static IMyRadioAntenna antenna;
        static IMyTextPanel console;
        static IMyProgrammableBlock definingModule; //assuming MBTransceiver will only ever be used with one defining module.
        static StringBuilder concatLite; //Im assuming ill invoke Clear() on this every time I finish using this.
        List <IMyProgrammableBlock> allModules;        
                
        List <object> nullCheckCollection = new List <object>()
        {
            antenna,
            console,
        };
       
        public void Initialise()
        {   
            bool allSystemsGo = true;
            int nullCount = default (int);              
            concatLite = new StringBuilder();
            antenna = GridTerminalSystem.GetBlockWithName (Names.ANTENNA) as IMyRadioAntenna;       
            console = GridTerminalSystem.GetBlockWithName (Names.SCREEN) as IMyTextPanel;
            allModules = new List <IMyProgrammableBlock>();
            GridTerminalSystem.GetBlocksOfType (allModules);

            CommunicationModel beanStalkModel = new CommunicationModel 
            (
                CommunicationModel.SupportedModelIdentities.BEANSTALK,

                new string[] 
                {
                    CommunicationModel.Audiences.BEANSTALK,
                    CommunicationModel.Audiences.LIMABEAN,
                    CommunicationModel.Audiences.KIDNEYBEAN,
                },

                new string[] {},

                new string[]
                {
                    CommunicationModel.PriorityActions.NETWORK,
                    CommunicationModel.PriorityActions.HELP,
                },

                new string[]
                {
                    CommunicationModel.Audiences.BEANSTALK,
                    CommunicationModel.Subjects.ENEMY,
                    CommunicationModel.Subjects.STUCK,
                }
            );

            CommunicationModel kidneyBeanModel = new CommunicationModel
            (
                CommunicationModel.SupportedModelIdentities.KIDNEYBEAN,

                new string[]
                {
                    CommunicationModel.Audiences.BEANSTALK,
                },

                new string[]
                {
                    CommunicationModel.JobActions.FOLLOW,
                },

                new string[]
                {
                    CommunicationModel.PriorityActions.ATTACK,
                    CommunicationModel.PriorityActions.GOTO,
                },

                new string[]
                {
                    CommunicationModel.Subjects.ENEMY,
                    CommunicationModel.Subjects.NEUTRAL,
                }
            );


            CommunicationModel limaBeanModel = new CommunicationModel
            (
                CommunicationModel.SupportedModelIdentities.LIMABEAN,
                
                new string[]
                {
                    CommunicationModel.Audiences.BEANSTALK,
                },

                new string[]
                {
                    CommunicationModel.JobActions.MINE,
                    CommunicationModel.JobActions.FOLLOW,
                },

                new string[]
                {
                    CommunicationModel.PriorityActions.GOTO,
                },

                new string[]
                {
                    CommunicationModel.Subjects.GOLD,
                    CommunicationModel.Subjects.ICE,
                    CommunicationModel.Subjects.IRON,
                    CommunicationModel.Subjects.MAGNESIUM,
                    CommunicationModel.Subjects.NICKEL,
                    CommunicationModel.Subjects.PLATINUM,
                    CommunicationModel.Subjects.SILICON,
                    CommunicationModel.Subjects.SILVER,
                    CommunicationModel.Subjects.URANIUM,
                }
            );

            for (int i = 0; i < allModules.Count; i++)
            {
                if (allModules[i].CustomName == CommunicationModel.Audiences.KIDNEYBEAN ||
                    allModules[i].CustomName == CommunicationModel.Audiences.LIMABEAN ||
                    allModules[i].CustomName == CommunicationModel.Audiences.BEANSTALK)
                {
                    definingModule = allModules[i];
                    nullCheckCollection.Add (definingModule);
                    allModules.Clear();
                    break; //each user of this script is only expected to have one of the defining modules.
                }

                else if (i == allModules.Count - 1) //assuming success state will break away before this happens.
                {
                    allSystemsGo = false;
                    Echo (Messages.NO_BLOCK);
                }
            }
            
            for (int i = 0; i < nullCheckCollection.Count; i++) 
            {
                if (nullCheckCollection[i] == null)
                {
                    nullCount++;
                }

                else if (i == nullCheckCollection.Count - 1 &&
                         nullCount != default (int))
                {
                    allSystemsGo = false;
                    Echo (Messages.NO_BLOCK + nullCount.ToString());
                }                
            } 
            compiled = allSystemsGo;
        }

        public void main (string serializedCommand)
        {              
            if (compiled)
            {                
                AnalyseForCommand (serializedCommand);  
                CheckForInternalCommunication();                  
            }

            else
            {
                Initialise();
            }
        }

        /// <summary>
        /// Analyses string input if there was a transmission received.
        /// </summary>
        /// <param name="input"></param>
        void AnalyseForCommand (string input)
        {
            string noEscapes = string.Format (@"{0}", input);
            string singleCase = noEscapes.ToUpper(); //assuming that an interaction model is all upper.
            Command possibleCommand = TryCreateCommand (singleCase);

            if (possibleCommand.IsEmpty == false &&
                possibleCommand.CommunicationScope == CommunicationModel.CommunicationScopes.EXTERNAL)
            {
                ApplyCommand (possibleCommand);
            }                    
        }

        /// <summary>
        ///Checks customdata storage for an internal communication.
        /// </summary>
        void CheckForInternalCommunication()
        {
            string[] procedureList = Me.CustomData.Split (new string[] { Names.NEW_LINE }, StringSplitOptions.RemoveEmptyEntries);

            if (procedureList.Length >= default (int))
            {
                Command possibleCommand = TryCreateCommand (procedureList[default (int)]);

                if (possibleCommand.IsEmpty == false &&
                    possibleCommand.CommunicationScope == CommunicationModel.CommunicationScopes.INTERNAL)
                {
                    ApplyCommand (possibleCommand);
                }       
            }
        }

        /// <summary>
        /// If conversion from string to Command was successful, returned command's property bool IsEmpty will be false.
        /// If conversion failed, it will be true.
        /// </summary>
        /// <param name="serialisedCommand"></param>
        /// <param name="possibleSuccessOutput"></param>
        Command TryCreateCommand (string serialisedCommand)
        {
            Command possibleSuccessOutput = new Command(); //Im returning a value type so it must be immutable.
            string[] sectionedString = serialisedCommand.Split (Names.SPACE);

            if (sectionedString.Length == Command.LENGTH)
            {
                Vector3D possibleVector;

                if (Vector3D.TryParse (sectionedString[Command.LENGTH - 1], out possibleVector))
                {
                    possibleSuccessOutput = new Command (sectionedString[0],
                                                         sectionedString[1],
                                                         sectionedString[2],
                                                         sectionedString[3],
                                                         possibleVector
                                                        );
                }
            }
            return possibleSuccessOutput;
        }

        void ApplyCommand (Command command)
        {
            string output = default (string); 

            switch (command.CommunicationScope)
            {
                case CommunicationModel.CommunicationScopes.INTERNAL:
                    output = serializeOutputCommand (command, CommunicationModel.CommunicationScopes.EXTERNAL);
                    antenna.TransmitMessage (output);
                    break;

                case CommunicationModel.CommunicationScopes.EXTERNAL:
                    if (command.SelectedAudience == definingModule.CustomName || //I only want to accept the command if it was directed to me.
                        command.SelectedAudience == Me.CubeGrid.EntityId.ToString())
                    {
                        output = serializeOutputCommand (command, CommunicationModel.CommunicationScopes.INTERNAL);
                        definingModule.CustomData += Names.NEW_LINE + output;
                    }
                    break;
            }

            if (output != default (string))
            {
                PrintToConsole (output);
            }            
        }

        /// <summary>
        /// An output command is a received message combined with a communication scope opposite of the received one.
        /// Method will serialize all that and return it to you.
        /// </summary>
        /// <param name="receivedCommand"></param>
        /// <param name="outputsScope"></param>
        /// <returns></returns>
        string serializeOutputCommand (Command receivedCommand, string outputsScope)
        {
            concatLite.Append (outputsScope);
            concatLite.Append (Names.SPACE);            
            concatLite.Append (receivedCommand.SelectedAudience);
            concatLite.Append (Names.SPACE);
            concatLite.Append (receivedCommand.Action);
            concatLite.Append (Names.SPACE);
            concatLite.Append (receivedCommand.Subject);
            concatLite.Append (Names.SPACE);
            concatLite.Append (receivedCommand.Location.ToString());
            string output = concatLite.ToString();
            concatLite.Clear();
            return output;
        }

        /// <summary>
        /// reads the paragraph in IMyTextPanel console and prints a new paragraph with message at the top.
        /// </summary>
        void PrintToConsole (string message)
        {
            string previousPrint = console.GetPublicText();
            concatLite.Append (Names.MY_CONSOLE_NAME);
            concatLite.Append (message);
            concatLite.Append (Names.NEW_LINE);
            concatLite.Append (previousPrint);
            console.WritePublicText (concatLite);
            concatLite.Clear();
            console.ShowPublicTextOnScreen();
        } 

        public void Save()
        {                       
        }
         
        /// <summary>
        /// A CommunicationModel defines the possible inputs of an external or internal communicator.
        /// Contained are a selection of every possible item a model could have and must be placed in the constructor when instantiating.
        /// magicbeans need to have a common understanding in their communications.         
        /// Not every bean is supposed to communicate with everything. If it has a model, it is allowed to communicate with it.
        /// It is assumed that each magicbean will have instantiated exactly the same models.
        /// The best way to interpret a command is check the received command's items match your corresponding model then figure out if that combination makes sense.
        /// </summary>
        class CommunicationModel
        {
            public enum SupportedModelIdentities
            {
                BEANSTALK,
                KIDNEYBEAN,
                LIMABEAN,
            } 
            
            public static class CommunicationScopes
            {
                public const string INTERNAL = "INTERNAL";
                public const string EXTERNAL = "EXTERNAL";
            }         

            /// <summary>
            /// entity id is a universal audience for a single entity.
            /// Audiences can be subjects.
            /// </summary>
            public static class Audiences 
            {                
                public const string BEANSTALK = "BEANSTALK";
                public const string KIDNEYBEAN = "KIDNEYBEAN";
                public const string LIMABEAN = "LIMABEAN";
            }            

            /// <summary>
            /// Job actions are the default state of a bean. When priorities are finished, they will continue with their job.
            /// Jobs can be assigned location which leaves the context up for the drone to determine.
            /// Beanstalk cannot be assigned a job since it is the master. It assigns it own jobs.
            /// </summary>
            public static class JobActions
            {
                public const string FOLLOW = "FOLLOW";
                public const string MINE = "MINE";    
            }

            public static class PriorityActions
            {
                public const string NETWORK = "NETWORK";
                public const string ATTACK = "ATTACK";
                public const string GOTO = "GOTO";                        
                public const string HELP = "HELP";
            }

            /// <summary>
            /// entity id can be a universal subject of a single entity.
            /// </summary>
            public static class Subjects
            {
                public const string PLATINUM = "PLATINUM";
                public const string GOLD = "GOLD";
                public const string SILVER = "SILVER";
                public const string SILICON = "SILICON";    
                public const string NICKEL = "NICKEL";
                public const string URANIUM = "URANIUM";
                public const string MAGNESIUM = "MAGNESIUM";
                public const string IRON = "IRON";
                public const string ICE = "ICE";                

                public const string ENEMY = "ENEMY";
                public const string NEUTRAL = "NEUTRAL";

                public const string STUCK = "STUCK";
            }    

            public readonly SupportedModelIdentities PersonalID;
            public readonly ReadOnlyCollection <string> PersonalAudiences;
            public readonly ReadOnlyCollection <string> PersonalJobs;
            public readonly ReadOnlyCollection <string> PersonalPriorities;
            public readonly ReadOnlyCollection <string> PersonalSubjects;
            
            public CommunicationModel (SupportedModelIdentities ModelsID, string[] modelsAudiences, string[] modelsJobs, string[] modelsPriorities, string[] modelsSubjects)
            {
                this.PersonalID = ModelsID;
                this.PersonalAudiences = Array.AsReadOnly (modelsAudiences);
                this.PersonalJobs = Array.AsReadOnly (modelsJobs);
                this.PersonalPriorities = Array.AsReadOnly (modelsPriorities);
                this.PersonalSubjects = Array.AsReadOnly (modelsSubjects);
            }                 
        }        

        public class Command
        {
            /// <summary>
            /// Will be true if you use the empty constructor overload.
            /// </summary>
            public readonly bool IsEmpty;
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

