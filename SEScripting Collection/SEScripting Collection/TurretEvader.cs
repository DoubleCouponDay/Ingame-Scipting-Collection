#region PRE_SCRIPT
using System;
using System.Collections.Generic;

using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;

public class TurretEvader : MyGridProgram
{
    #region script

    List <IMyTerminalBlock> thrusters = new List <IMyTerminalBlock>();
    List <IMyTerminalBlock> left = new List <IMyTerminalBlock>();
    List <IMyTerminalBlock> up = new List <IMyTerminalBlock>();
    List <IMyTerminalBlock> right = new List <IMyTerminalBlock>();
    List <IMyTerminalBlock> down = new List <IMyTerminalBlock>();
    
    enum frequency
    {
        millisecond,
        tenth,
        half,
        second
    }

    frequency currentFrequency = frequency.half;
    int tickCount;
    int tickMax;

    bool first_time = true;
    int thrusterState;

    const float maxOverride = 500000.0f;
    const float gravityFactor = 1000.0f;
    bool isAtmospheric;

    void Main (string argument)
    {
        if (first_time)
        {
            first_time = false;
            Initialize ();
        }

        if (argument == "run")
        {
            isAtmospheric = false;
        }

        else if(argument == "run atmo")
        {
            isAtmospheric = true;
        }
    
        else if (argument == "stop")
        {
            StopVessel ();
            return;
        }

        else
        {
            ParseArgument(argument);
        }        

        if (tickCount <= 0)
        {
            tickCount = tickMax;
            ApplyState();
        }
        
        tickCount--;
    }

    //this function will fetch objects from terminal only once in order to save performance.
    void Initialize()
    {            
        GridTerminalSystem.GetBlocksOfType <IMyThrust> (thrusters);
        thrusterState = 1;

        for (int a = 0; a < thrusters.Count; a++)
        {
            thrusters[a].ApplyAction ("OnOff_On");
            string blocksOrientation = thrusters[a].Orientation.ToString();
            string[] refinedResult = blocksOrientation.Split (',');
            refinedResult = refinedResult[0].Split (':');

            switch (refinedResult[1])
            {
                case "Up":
                    up.Add (thrusters[a]);
                    break;
                    
                case "Down":
                    down.Add (thrusters[a]);
                    break;

                case "Left":
                    left.Add (thrusters[a]);
                    break;

                case "Right":
                    right.Add (thrusters[a]);
                    break;
            }
        }
    }

    void ParseArgument(string input)
    {
        if (input == string.Empty)
        {
            return;
        }
        var parsed = Enum.Parse(typeof(frequency), input);
        tickMax = 0;

        switch (parsed)
        {
            case frequency.millisecond:
                Runtime.UpdateFrequency = UpdateFrequency.Update1;
                break;

            case frequency.tenth:
                Runtime.UpdateFrequency = UpdateFrequency.Update100;
                break;

            case frequency.half:
                Runtime.UpdateFrequency = UpdateFrequency.Update100;
                tickMax = 5;
                break;

            case frequency.second:
                Runtime.UpdateFrequency = UpdateFrequency.Update100;
                tickMax = 10;
                break;

            default:
                throw new Exception($"frequency not accounted for: {parsed}");
        }
    }

    //this function cycles through four states which thrust the decoy in a circle, while in the atmosphere.
    void ApplyState()
    {
        switch (thrusterState)	
        {
            case 1:
                thrusterState = 2;
                ChangeOverride (left, true);
                ChangeOverride (down, false);
                Echo ("left");
                break;
        
            case 2:
                thrusterState = 3;
                ChangeOverride (up, true, true);
                ChangeOverride (left, false);
                Echo ("up");
                break;
            
            case 3:
                thrusterState = 4;
                ChangeOverride (right, true);
                ChangeOverride (up, false);
                Echo ("right");
                break;
            
            case 4:
                thrusterState = 1;
                ChangeOverride (down, true);
                ChangeOverride (right, false);
                Echo ("down");
                break;
        }
    }

    //this function will change thrust override based on argument inputs.
    void ChangeOverride (List <IMyTerminalBlock> thruster_face, bool isOn, bool goingDown = false)
    {
        for (int a = 0; a < thruster_face.Count; a++)
        {
            if (isOn)
            {
                if (isAtmospheric && goingDown)
                {
                    float reducedOverride = maxOverride / gravityFactor;
                    thruster_face[a].SetValueFloat("Override", reducedOverride);
                }

                else
                {
                    thruster_face[a].SetValueFloat("Override", maxOverride);
                }
            }	
        
            else
            {
                thruster_face[a].SetValueFloat("Override", 0.0F);
            }
        }
    }

    //this function resets the ship to its normal state if the user inputs a certain string.
    void StopVessel()
    {
        for (int a = 0; a < thrusters.Count; a++)
        {
            thrusters[a].SetValueFloat ("Override", 0.0f); 
            thrusters[a].ApplyAction ("OnOff_On");
        }
        Runtime.UpdateFrequency = UpdateFrequency.None;
        tickMax = 0;
    }
    #region script
}
