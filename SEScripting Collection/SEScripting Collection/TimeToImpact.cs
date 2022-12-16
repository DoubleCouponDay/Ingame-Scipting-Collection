
using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using VRage.Game.ModAPI.Ingame;

public class TimeToImpact : MyGridProgram
{
#region in-game
    static class Names
    {
        public const string FORWARD_CAM = "FORWARD CAM";
        public const string DOWN_CAM = "DOWN CAM";
        public const string TIMER = "IMPACT SCRIPT TIMER";
        public const string FAST_REFRESH = "TriggerNow";
        public const string SLOW_REFRESH = "Start";
    }
        
    static class Messages
    {
        public const string INITIAL_GREETING = "Enter the name of the screen which TimeToImpact will append, then press run.";        
        public const string BEYOND_ANGLE = "Angle of raycast is beyond vision.";
    }        

    enum RefreshTimes
    {
        Slow,
        Fast,
    }
    const double PROXIMITY_BOUNDARY = 20.0F;
    const float AT_REST = 0.0F;
    const string EMPTY = "";
                
    bool initialised;    
    string currentRefreshRate;  
    double previousDistance;

    static IMyCameraBlock forwardCam;
    static IMyCameraBlock downCam;
    static IMyTextPanel outputDisplay;
    static IMyTimerBlock timer;  
    static IMyShipController controlReadings;   
          
    List <IMyTerminalBlock> queryResults = new List <IMyTerminalBlock>();
    List <string> outputLines = new List <string>();

    readonly object[] nullCheckCollection = {
        forwardCam,
        downCam,  
        outputDisplay,
        timer,
        controlReadings,
    };

    readonly List <IMyCameraBlock> cameras = new List<IMyCameraBlock>() {
        forwardCam,
        downCam,
    };      

/* Objectives:
* + script must change its refresh rate based on how much time is left till impact,
* + script must continue where it left off even after losing power,
*/
    public TimeToImpact()
    {   
        currentRefreshRate = Names.SLOW_REFRESH;     
        Echo (Messages.INITIAL_GREETING);            
    }

    public void Main (string input)
    {           
        if (initialised == false)
        {                
            TryBoot (input);    
        }

        else
        {
            for (int i = default (int); i < cameras.Count; i++)
            {                
                if (FireSelectedCam (cameras[i] as IMyCameraBlock))
                {
                    break;
                }
            }
            Save();
        }
    }

    void TryBoot (string inputScreenName)
    {
        if (inputScreenName == EMPTY)
        { 
            Echo (Messages.INITIAL_GREETING);
        }

        else
        {
            int nullCount = default (int);
            forwardCam = GridTerminalSystem.GetBlockWithName (Names.FORWARD_CAM) as IMyCameraBlock;
            downCam = GridTerminalSystem.GetBlockWithName (Names.DOWN_CAM) as IMyCameraBlock;
            outputDisplay = GridTerminalSystem.GetBlockWithName (inputScreenName) as IMyTextPanel;
            timer = GridTerminalSystem.GetBlockWithName (Names.TIMER) as IMyTimerBlock;
            GridTerminalSystem.GetBlocksOfType <IMyShipController> (queryResults);

            if (queryResults.Count > default (int))
            {
                controlReadings = queryResults[default (int)] as IMyShipController;
            }

            for (int i = default (int); i < nullCheckCollection.Length; i++)
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

            else if (nullCount != default (int))
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

    /// <summary>
    ///
    /// </summary>
    /// <param name="cameraChosen"></param>
    /// <returns>True if hit obstacle.</returns>
    bool FireSelectedCam (IMyCameraBlock cameraChosen)            
    {   
        bool outcome = default (bool);
                        
        if (cameraChosen.CanScan (previousDistance))
        {
            outcome = true;
            Vector3D currentVector = controlReadings.GetShipVelocities().LinearVelocity;
            MyDetectedEntityInfo testFire = cameraChosen.Raycast (currentVector);
                
            if (testFire.IsEmpty() == false)
            {
                Vector3D hitPosition = (Vector3D) testFire.HitPosition;
                previousDistance = Vector3D.Distance (controlReadings.GetPosition(), hitPosition);
                double speed = currentVector.Length();
                double timeToImpact = previousDistance / speed;
                PrintAnalysis (speed, previousDistance, timeToImpact, EMPTY);

                if (previousDistance < PROXIMITY_BOUNDARY)
                {
                    AdjustCastTime (RefreshTimes.Fast);
                }

                else
                {
                    AdjustCastTime (RefreshTimes.Slow);
                }
            } 

            else
            {
                PrintAnalysis (float.NaN, float.NaN, float.NaN, Messages.BEYOND_ANGLE);
            }
        }

        else //scans will regularly be beyond the charged range. should not print in this scenario.
        {
            outcome = false;
        }
        return outcome;
    }

    void AdjustCastTime (RefreshTimes inputRefreshTime)
    {
        switch (inputRefreshTime)
        {
            case RefreshTimes.Slow:
                currentRefreshRate = Names.SLOW_REFRESH;
                break;

            case RefreshTimes.Fast:
                currentRefreshRate = Names.FAST_REFRESH;
                break;
        }
    }
        
    void PrintAnalysis (double velocity, double destinationDistance, double timeOfArrival, string comment)
    {
        outputLines.Clear();

        if (comment != Messages.BEYOND_ANGLE)
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
            outputLines.Add (Messages.BEYOND_ANGLE);
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
    #endregion in-game
}    
