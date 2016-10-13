#region PRE_SCRIPT
using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;

namespace Ingame_Scripting_Collection
{
    public class PhonemeFinder : MyGridProgram
    {
#endregion PRE_SCRIPT
        internal const float max_volume = 1.0f;
        internal const float min_volume = 0.0f;

        List <IMyTerminalBlock> speakers = new List <IMyTerminalBlock> ();
        IMyTerminalBlock timer;

        internal List <int> delay = new List <int> ();
        internal List <int> duration = new List <int> ();
        internal bool first_time = true;
        internal bool end_program = false;
        internal bool playing = false;
        internal bool count_delay = false;
        internal bool count_duration = false;

        void Main (string argument)
        {
            if (first_time == true)
            {
                Initialize ();
            }  

            switch (argument) //with this logic, it will stop looping if you argue "stop" but you will need to press run when you argue "run".
            {
                case "run;": 
			        end_program = false;
			        break; 
        
                case "stop;":
			        end_program = true;
                    Stop ();
                    break;

		        default:
                    if (argument.Contains (";"))
                    {
                        Play (argument);
                    }
            
                    else if (!argument.Contains (";") || string.IsNullOrEmpty(argument))
                    {
                        Stop ();
                    }    
			        break;
            }
	
            if (end_program == false)
            {
                timer.ApplyAction ("TriggerNow");
            }
        }

        void Initialize ()
        {
            first_time = false;  
            GridTerminalSystem.GetBlocksOfType <IMySoundBlock> (speakers);
            timer = GridTerminalSystem.GetBlockWithName ("script timer");
    
            for (int a = 0; a < speakers.Count;a++)
            {
                speakers[a].SetValueFloat ("VolumeSlider", 0.0f);
                speakers[a].ApplyAction ("PlaySound");
            }

        }

        void Play (string input)
        {
            string[] fragments = input.Split ('/'); //split takes '', not "".
            int speaker_choice = Convert.ToInt32 (fragments[0]); //speaker choice and fragments need to be visible to the entire method.
            delay.Add (Convert.ToInt32 (fragments[1]));
    
            if (!fragments[2].Contains (";"))
            {
                duration.Add (Convert.ToInt32 (fragments[2]));
            }
    
            else
            {
                return;
            }
    
            if (playing == false)
            {
                playing = true;
                count_delay = true; 
                speakers[speaker_choice].ApplyAction ("StopSound");
                speakers[speaker_choice].SetValueFloat ("VolumeSlider", min_volume);  
                speakers[speaker_choice].ApplyAction ("PlaySound");   
            }
    
            if (playing == true)
            {
                if (delay[0] == 0 && count_delay == true)
                {
                    count_delay = false;
                    count_duration = true;
                    speakers[speaker_choice].SetValueFloat ("VolumeSlider", max_volume);
                }
        
                else if (count_delay == true)
                {
                    delay[0]--;
                }  

                if (duration[0] == 0 && count_duration == true)
                {
                    count_duration = false;
                    speakers[speaker_choice].SetValueFloat ("VolumeSlider", min_volume);  
                }
        
                else if (count_duration == true)
                {
                    duration[0]--;
                }
            }
        }

        void Stop ()
        {
            playing = false;
            count_delay = false;
            count_duration = false;
            delay.Clear ();
            duration.Clear ();
    
            for (int a = 0; a < speakers.Count; a++)
            {
                speakers[a].ApplyAction ("StopSound");
            }    
        }
#region POST_SCRIPT
    }    
}
#endregion POST_SCRIPT