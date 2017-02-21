
using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using VRage.Game;
using System.Collections.ObjectModel;

namespace MagicBeans18
{
    public class Program : MyGridProgram
    {
#region in-game
        struct Names
        {
            public const long NO_OWNER = 0; 
            public const string FAST_REFRESH = "TriggerNow";
            public const string SLOW_REFRESH = "Start";
        }

        public Program()
        {

        }

        public void Main()
        {

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
