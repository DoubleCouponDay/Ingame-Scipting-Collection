#region PRE_SCRIPT
using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace Ingame_Scripting_Collection
{
    public class TimeToImpact : MyGridProgram
    {
#endregion PRE_SCRIPT
        const string FORWARD_CAM_NAME = "FORWARD CAM";
        const string DOWN_CAM_NAME = "DOWN CAM";
        const string TIMER_NAME = "IMPACT SCRIPT TIMER";
        const string FAST_REFRESH = "TriggerNow";
        const string SLOW_REFRESH = "Start";
        const string INITIAL_GREETING = "Enter the name of the screen which TimeToImpact will append, then press run.";        
        const string BEYOND_ANGLE = "Angle of raycast is beyond vision.";
        const float AT_REST = 0.0F;

        static IMyCameraBlock forwardCam;
        static IMyCameraBlock downCam;
        static IMyTextPanel outputDisplay;
        static IMyTimerBlock timer;  
        static IMyShipController controlReadings;   
          
        List <IMyTerminalBlock> queryResults;
        List <string> outputLines = new List <string>();

        object[] nullCheckCollection = {
            forwardCam,
            downCam,  
            outputDisplay,
            timer,
            controlReadings,
        };

        object[] cameras = {
            forwardCam,
            downCam,
        };

        bool initialised;
        string LCDBlockName;        
        string currentRefreshRate;        

/* Objectives:
 * + script must change its refresh rate based on how much time is left till impact,
 * + script must continue where it left off even after losing power,
 */

      //public TimeToImpact()
        public Program()
        {   
            currentRefreshRate = SLOW_REFRESH;     
            queryResults = new List <IMyTerminalBlock>();    
            Echo (INITIAL_GREETING);            
        }

        public void Main (string input)
        {           
            if (initialised == false)
            {                
                TryBoot (input);    
            }

            else
            {
                MyShipVelocities currentVector = controlReadings.GetShipVelocities();
                bool laserHitFailed = true;

                for (int i = 0; i < cameras.Length; i++)
                {                
                    if (FireSelectedCam (cameras[i] as IMyCameraBlock, currentVector))
                    {
                        laserHitFailed = false;
                        break;
                    }                    
                }

                if (laserHitFailed)
                {
                    Print (AT_REST, AT_REST, AT_REST, BEYOND_ANGLE);
                }
                Save();
            }
        }

        void TryBoot (string inputScreenName)
        {
            if (inputScreenName == "")
            { 
                Echo (INITIAL_GREETING);
            }

            else
            {
                int nullCount = 0;
                forwardCam = GridTerminalSystem.GetBlockWithName (FORWARD_CAM_NAME) as IMyCameraBlock;
                downCam = GridTerminalSystem.GetBlockWithName (DOWN_CAM_NAME) as IMyCameraBlock;
                outputDisplay = GridTerminalSystem.GetBlockWithName (inputScreenName) as IMyTextPanel;
                timer = GridTerminalSystem.GetBlockWithName (TIMER_NAME) as IMyTimerBlock;
                GridTerminalSystem.GetBlocksOfType <IMyShipController> (queryResults);

                if (queryResults.Count > 0)
                {
                    controlReadings = queryResults[0] as IMyShipController;
                }

                for (int i = 0; i < nullCheckCollection.Length; i++)
                {
                    if (nullCheckCollection[i] == null)
                    {
                        nullCount++;
                    }
                }                           

                if (outputDisplay == null)
                {
                    Echo ("ERROR: Could not find screen with that name; Try again.");
                }

                else if (nullCount != 0)
                {
                    Echo ("ERROR: One of the blocks required has the wrong name or does not exist.");
                    Echo ("");
                    Echo ("Number of block name faults: " + nullCount.ToString() + "/" + nullCheckCollection.Length);
                }

                else 
                {
                    initialised = true;
                    forwardCam.EnableRaycast = true;
                    downCam.EnableRaycast = true;
                }
            }            
        }

        bool FireSelectedCam (IMyCameraBlock cameraChosen, MyShipVelocities currentVector)            
        {   
            MyDetectedEntityInfo testFire = downCam.Raycast (currentVector.LinearVelocity);

            if (testFire.IsEmpty() == false)
            {
                Vector3D? hitPosition = testFire.HitPosition;
            } 
        }

        void AdjustCastTime()
        {

        }
        
        void Print (float velocity, float destinationDistance, float timeOfArrival, string comment)
        {
            outputLines.Clear();

            if (comment != BEYOND_ANGLE)
            {
                string velocityBuild = "\n velocity: " + velocity.ToString(); 
                string destinationBuild = "\n distance: " + destinationDistance.ToString();
                string arrivalBuild = "\n TimeToImpact: " + timeOfArrival.ToString();
                outputLines.Add (velocityBuild);
                outputLines.Add (destinationBuild);
                outputLines.Add (arrivalBuild);
                Echo (velocityBuild);
                Echo (destinationBuild);
                Echo (arrivalBuild);
                outputDisplay.WritePublicText (velocityBuild, true);
                outputDisplay.WritePublicText (destinationBuild, true);
                outputDisplay.WritePublicText (arrivalBuild, true);
            }

            else
            {
                outputLines.Add (BEYOND_ANGLE);
            }

            for (int i = 0; i < outputLines.Count; i++)
            {
                Echo (outputLines[i]);
                outputDisplay.WritePublicText (outputLines[i], true);
            }
        }

        public void Save()
        {
            timer.ApplyAction (currentRefreshRate);
        }
#region POST_SCRIPT
    }    
}
#endregion POST_SCRIPT
