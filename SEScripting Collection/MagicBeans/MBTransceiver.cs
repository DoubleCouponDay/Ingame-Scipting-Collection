

//TEsting edit on Tactical Anarchy Branch
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
        List <InteractionModel> models;
        
                
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

            InteractionModel magicBeanModel = new InteractionModel 
            (
                InteractionModel.SupportedModels.MAGICBEAN,

                new string[] 
                {
                    InteractionModel.Audiences.MAGICBEANS,
                    InteractionModel.Audiences.LIMABEANS,
                    InteractionModel.Audiences.KIDNEYBEANS,
                },

                new string[] { },

                new string[]
                {
                    InteractionModel.PriorityActions.NETWORK,
                    InteractionModel.PriorityActions.BACKUP,
                },

                new string[]
                {
                    InteractionModel.Subjects.ENEMY,
                }
            );

            InteractionModel kidneyBeanModel = new InteractionModel
            (
                InteractionModel.SupportedModels.KIDNEYBEAN,

                new string[]
                {
                    InteractionModel.Audiences.MAGICBEANS,
                },

                new string[]
                {
                    InteractionModel.JobActions.FOLLOW,
                },

                new string[]
                {
                    InteractionModel.PriorityActions.ATTACK,
                    InteractionModel.PriorityActions.GOTO,
                },

                new string[]
                {
                    InteractionModel.Subjects.ENEMY,
                    InteractionModel.Subjects.NEUTRAL,
                }
            );


            InteractionModel limaBeanModel = new InteractionModel
            (
                InteractionModel.SupportedModels.LIMABEAN,
                
                new string[]
                {
                    InteractionModel.Audiences.MAGICBEANS,
                },

                new string[]
                {
                    InteractionModel.JobActions.MINE,
                    InteractionModel.JobActions.FOLLOW,
                },

                new string[]
                {
                    InteractionModel.PriorityActions.GOTO,
                },

                new string[]
                {
                    InteractionModel.Subjects.GOLD,
                    InteractionModel.Subjects.ICE,
                    InteractionModel.Subjects.IRON,
                    InteractionModel.Subjects.MAGNESIUM,
                    InteractionModel.Subjects.NICKEL,
                    InteractionModel.Subjects.PLATINUM,
                    InteractionModel.Subjects.SILICON,
                    InteractionModel.Subjects.SILVER,
                    InteractionModel.Subjects.URANIUM,
                }
            );

            for (int i = 0; i < allModules.Count; i++)
            {
                if (allModules[i].CustomName == InteractionModel.Audiences.KIDNEYBEANS ||
                    allModules[i].CustomName == InteractionModel.Audiences.LIMABEANS ||
                    allModules[i].CustomName == InteractionModel.Audiences.MAGICBEANS)
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
                possibleCommand.CommunicationScope == InteractionModel.CommunicationScopes.EXTERNAL)
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
                    possibleCommand.CommunicationScope == InteractionModel.CommunicationScopes.INTERNAL)
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
                case InteractionModel.CommunicationScopes.INTERNAL:
                    output = serializeOutputCommand (command, InteractionModel.CommunicationScopes.EXTERNAL);
                    antenna.TransmitMessage (output);

                    break;

                case InteractionModel.CommunicationScopes.EXTERNAL:
                    if (command.SelectedAudience == definingModule.CustomName ||
                        command.SelectedAudience == Me.CubeGrid.EntityId.ToString())
                    {
                        output = serializeOutputCommand (command, InteractionModel.CommunicationScopes.INTERNAL);
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

        public void Save() //called by game on session close.
        {                       
        }

        /// <summary>
        /// A model defines the possible inputs of an external or internal communicator.
        /// </summary>
        class InteractionModel
        {
            public enum SupportedModels
            {
                MAGICBEAN,
                KIDNEYBEAN,
                LIMABEAN,
            }

            public enum ScopesMatchingEnum
            {
                INTERNAL,
                EXTERNAL,
            }   
            
            public static class CommunicationScopes
            {
                public const string INTERNAL = "INTERNAL";
                public const string EXTERNAL = "EXTERNAL";
            }         

            /// <summary>
            /// entity id is a universal audience type for a single entity.
            /// Audiences can be subjects.
            /// Some of these audiences are internal modules which dont require radio communication.
            /// </summary>
            public static class Audiences 
            {                
                public const string MAGICBEANS = "MAGICBEANS";
                public const string KIDNEYBEANS = "KIDNEYBEANS";
                public const string LIMABEANS = "LIMABEANS";
            }            

            public static class JobActions
            {
                public const string FOLLOW = "FOLLOW";
                public const string MINE = "MINE";    
                public const string BUILD = "BUILD";                         
            }

            public static class PriorityActions
            {
                public const string NETWORK = "NETWORK";
                public const string ATTACK = "ATTACK";
                public const string GOTO = "GOTO";                        
                public const string BACKUP = "BACKUP";
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
            }    

            public readonly SupportedModels PersonalID;
            public readonly ReadOnlyCollection <string> PersonalAudiences;
            public readonly ReadOnlyCollection <string> PersonalJobs;
            public readonly ReadOnlyCollection <string> PersonalPriorities;
            public readonly ReadOnlyCollection <string> PersonalSubjects;
            
            /// <summary>
            /// In this class there are selection tables for each of these string[]'s. You can define the scope of your model.
            /// </summary>
            /// <param name="modelsID">Can help you to distinguish models from each other. Should be unique.</param>
            /// <param name="modelsAudiences"></param>
            /// <param name="modelsJobs"></param> 
            /// <param name="modelsPriorities"></param>
            /// <param name="modelsSubjects"></param>
            public InteractionModel (SupportedModels ModelsID, string[] modelsAudiences, string[] modelsJobs, string[] modelsPriorities, string[] modelsSubjects)
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
            
            /// <summary>
            /// Communication scope is not added to the serialised command "Formatted".
            /// This is to ensure it can be easily appended to the front when you need to.
            /// </summary>
            public string Formatted { get; private set; }                
            
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

