#region PRE_SCRIPT
using System.Collections.Generic;

using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;

namespace Ingame_Scripting_Collection7
{
    public class Program : MyGridProgram
    {
        #endregion PRE_SCRIPT
        //auto turret evasion script
        //
        //bug 1: if you travel too fast towards the turret, it will catch your predictable vector.

        List <IMyTerminalBlock> thrusters = new List <IMyTerminalBlock>();
        List <IMyTerminalBlock> left = new List <IMyTerminalBlock>();
        List <IMyTerminalBlock> up = new List <IMyTerminalBlock>();
        List <IMyTerminalBlock> right = new List <IMyTerminalBlock>();
        List <IMyTerminalBlock> down = new List <IMyTerminalBlock>();
        IMyTerminalBlock timer;
        bool first_time = true;
        bool end_program = true;
        int state = 0;

        void Main (string argument)
        {
            if (first_time)
            {
                first_time = false;
                Initialize ();
            }

            if (argument == "run")
            {
                end_program = false;
            }
    
            else if (argument == "stop")
            {
                StopVessel ();
            }

            if (end_program == false)
            {
                ApplyState ();
                timer.ApplyAction ("Start");
            }
        }

        //this function will fetch objects from terminal only once in order to save performance.
        void Initialize()
        {            
            GridTerminalSystem.GetBlocksOfType <IMyThrust> (thrusters);
            timer = GridTerminalSystem.GetBlockWithName ("script timer");
            state = 1;

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

        //this function cycles through four states which thrust the decoy in a circle, while in the atmosphere.
        void ApplyState()
        {
            switch (state)	
            {
                case 1:
                    state = 2;
                    ChangeOverride (left, "on", "no");
                    ChangeOverride (down, "off", "no");
                    ChangeOverride (up, "on", "yes"); // i have to separately turn on up thrusters so that damps will affect down.
                    Echo ("left");
                    break;
        
                case 2:
                    state = 3;
                    ChangeOverride (up, "on", "no");
                    ChangeOverride (left, "off", "no");
                    Echo ("up");
                    break;
            
                case 3:
                    state = 4;
                    ChangeOverride (right, "on", "no");
                    ChangeOverride (up, "off", "no");
                    Echo ("right");
                    break;
            
                case 4:
                    state = 1;
                    ChangeOverride (down, "on", "no");
                    ChangeOverride (up, "off", "yes"); // i have to separately turn off up thrusters so that damps wont affect down.
                    ChangeOverride (right, "off", "no");
                    Echo ("down");
                    break;
            }
        }

        //this function will change thrust override based on argument inputs.
        void ChangeOverride (List <IMyTerminalBlock> thruster_face, string on_off, string damps_toggle)
        {
            for (int a = 0; a < thruster_face.Count; a++)
            {
                if (on_off == "off" && damps_toggle == "no")
                {
                    thruster_face[a].SetValueFloat ("Override", 0.0F);
                }	
        
                else if (on_off == "on" && damps_toggle == "no")
                {
                    thruster_face[a].SetValueFloat ("Override", 80000.0F);
                }
        
                else if (on_off == "off" && damps_toggle == "yes")
                {
                    up[a].ApplyAction ("OnOff_Off");
                }
        
                else if (on_off == "on" && damps_toggle == "yes")
                {
                    up[a].ApplyAction ("OnOff_On");
                }
            }
        }

        //this function resets the ship to its normal state if the user inputs a certain string.
        void StopVessel()
        {
            end_program = true;
    
            for (int a = 0; a < thrusters.Count; a++)
            {
                thrusters[a].SetValueFloat ("Override", 0.0f); 
                thrusters[a].ApplyAction ("OnOff_On");
            }
        }
#region POST_SCRIPT
    }    
}
#endregion POST_SCRIPT