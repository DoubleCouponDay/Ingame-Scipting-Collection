
using System.Collections.Generic;

using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace Ingame_Scripting_Collection6
{
    public class Program : MyGridProgram
    {
#region in-game
        const string timersName = "battery display timer";       
        const string displaysName = "SFM readings";               
        const int kilowatts = 1000;
        const int brownOutBuffer = 100;

        bool endProgram;
        int switchingInProgress;
        decimal totalPowerStored;
        decimal totalCapacity;

        List <IMySolarPanel> panels = new List <IMySolarPanel>();
        List <IMyBatteryBlock> batteries = new List <IMyBatteryBlock>();
        List <IMyReactor> reactors = new List <IMyReactor>();
        IMyTimerBlock timer;
        IMyTextPanel display;

        public Program()
        {
            endProgram = BootUp();
        }

        public bool BootUp()
        {
            bool justEndMyShitUp = false;
            totalPowerStored = 0.0M;
            totalCapacity = 0.0M;
            timer = GridTerminalSystem.GetBlockWithName (timersName) as IMyTimerBlock;
            display = GridTerminalSystem.GetBlockWithName (displaysName) as IMyTextPanel;
            GridTerminalSystem.GetBlocksOfType <IMyBatteryBlock> (batteries);   
            GridTerminalSystem.GetBlocksOfType <IMyReactor> (reactors);   
                     
            List<IMyTerminalBlock> nullCheck = new List <IMyTerminalBlock>();
            nullCheck.Add (timer);
            nullCheck.Add (display);
            nullCheck.Add (batteries[0]);
            string[] errors = {"+TIMER HAS THE WRONG NAME OR DOES NOT EXIST.",
                               "+DISPLAY SCREEN HAS THE WRONG NAME OR DOES NOT EXIST.",
                               "+YOU DONT HAVE ANY BATTERIES."};

            for (int i = 0; i < nullCheck.Count; i++)
            {
                if (nullCheck[i] == null)
                {
                    justEndMyShitUp = true;
                    Echo (errors[i]);
                }
            }            
            return justEndMyShitUp;
        }

        public void Main (string input)
        {                       
            if (endProgram == false)
            {
                Save();
            }            
        }

        public void Save() //save the state of the script
        {
            timer.ApplyAction ("Start");
            totalPowerStored = 0.0M;
            totalCapacity = 0.0M;
        }
#endregion in-game
    }
}
