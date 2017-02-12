
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
            public const string REPEATER = "MBRepeater";    
            public const string NEW_LINE = "\n";
        }

        struct Messages
        {
            public const string NO_BLOCK = "TRANSCEIVER ERROR: Block not found; ";
        } 

        bool compiled = false;
        string serialMemory;

        static IMyRadioAntenna antenna;
        static IMyTextPanel console;
        static IMyProgrammableBlock definingModule;
        List <IMyProgrammableBlock> allModules;
        List <InteractionModel> models;
        public static readonly StringBuilder concatLite = new StringBuilder(); //Im assuming ill invoke Clear() on this every time I finish using this.
                
        List <object> nullCheckCollection = new List <object>()
        {
            antenna,
            console,
        };
       
        public void Initialise()
        {   
            int nullCount = default (int);              
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
            }
            
            for (int i = 0; i < nullCheckCollection.Count; i++) 
            {
                if (nullCheckCollection[i] == null)
                {
                    nullCount++;
                }
            } 
            
            if (nullCount != default (int))
            {
                Echo (Messages.NO_BLOCK + nullCount.ToString());
            }

            else
            {
                compiled = true;
            }
        }

        public void main (string serializedCommand)
        {              
            if (compiled)
            {
                AnalyseForCommand (serializedCommand);                    
            }

            else
            {
                Initialise();
            }
        }

        void AnalyseForCommand (string input)
        {
            string noEscapes = string.Format (@"{0}", input);
            string singleCase = noEscapes.ToUpper();            
            string[] sectionedUp = singleCase.Split (' ');

            if (sectionedUp.Length == Command.COMMAND_LENGTH)
            {
                Vector3D possibleVector;

                if (Vector3D.TryParse (sectionedUp[Command.COMMAND_LENGTH - 1], out possibleVector))
                {
                    Command orderReceived = new Command (sectionedUp[0],
                                                         sectionedUp[1],
                                                         sectionedUp[2],
                                                         sectionedUp[3],
                                                         possibleVector
                                                        );
                    ApplyCommand (orderReceived);
                }
            }
        }

        /// <summary>
        /// Method will convert an internally received message to an external tranmission.
        /// Similarly, an external transmission received will be converted to an internal message between modules.
        /// </summary>
        /// <param name="command"></param>
        void ApplyCommand (Command command)
        {
            if (command.CommunicationScope == InteractionModel.CommunicationScopes.INTERNAL)
            {
                concatLite.Append (InteractionModel.CommunicationScopes.EXTERNAL);
                concatLite.Append (" ");
                concatLite.Append (command.Formatted);
                string outPacket = concatLite.ToString();
                concatLite.Clear();
                antenna.TransmitMessage (outPacket);
            }

            else if (command.CommunicationScope == InteractionModel.CommunicationScopes.EXTERNAL)
            {
                if (command.SelectedAudience == definingModule.CustomName ||
                    command.SelectedAudience == Me.CubeGrid.EntityId.ToString())
                {
                    concatLite.Append (InteractionModel.CommunicationScopes.EXTERNAL);
                    concatLite.Append (" ");
                    concatLite.Append (command.Formatted);
                    string inPacket = concatLite.ToString();
                    concatLite.Clear();
                    definingModule.CustomData += Names.NEW_LINE + inPacket; //Alternative to TryRun (string) because that way cant run an already running PB. I cant predict that.

                }
            }
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

            public static class CommunicationScopes
            {
                public const string EXTERNAL = "EXTERNAL";
                public const string INTERNAL = "INTERNAL";
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
            public const int COMMAND_LENGTH = 5;

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

            public Command (string communicationScope, string selectedAudience, string action, string subject, Vector3D location)
            {               
                this.CommunicationScope = communicationScope;     
                this.SelectedAudience = selectedAudience;
                this.Action = action;
                this.Subject = subject;
                this.Location = location;

                concatLite.Append (SelectedAudience);
                concatLite.Append (" ");
                concatLite.Append (Action);
                concatLite.Append (" ");
                concatLite.Append (Subject);
                concatLite.Append (" ");
                concatLite.Append (location.ToString());
                Formatted = concatLite.ToString();
                concatLite.Clear(); //think about other methods using the same stringBuilder. clean up after yourself.
            } 
        }   
#endregion in-game
    }    
}

