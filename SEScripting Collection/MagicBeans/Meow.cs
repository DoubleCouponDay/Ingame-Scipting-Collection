#region PRE_SCRIPT
using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace MagicBeans
{
    class Meow : MyGridProgram
    {
#endregion PRE_SCRIPT
#region in-game

/*instructions: 
 * place sensor and timer on ship.
 * place PB and warhead on merge block so that they drop when the merge block detaches.
 * place power source on ship NOT ON MERGE BLOCK.
 * sensor must trigger the merge block if it detects hostile.
 */
        const float COUNTDOWN = 3.0F;

        IMySensorBlock sensor;
        IMyShipMergeBlock mergeBlock;
        IMyWarhead bomb;
        IMyTimerBlock timer;
/*
        public Program()
        {
            sensor = GridTerminalSystem.GetBlockWithName ("SENSOR") as IMySensorBlock;
            mergeBlock = GridTerminalSystem.GetBlockWithName ("MERGE_BLOCK") as IMyShipMergeBlock;
            bomb = GridTerminalSystem.GetBlockWithName ("BOMB") as IMyWarhead;
            timer = GridTerminalSystem.GetBlockWithName ("TIMER") as IMyTimerBlock;
        }
*/
        public void main()
        {
            if (sensor != null)
            {
                if (sensor.IsFunctional)
                {
                    bomb.SetValueFloat ("Countdown", COUNTDOWN);    
                    bomb.SetValueBool ("Safety", false);
                    bomb.ApplyAction ("StartCountdown");
                    
                }

                else
                {
                    DetachBrain();
                }
            }

            else
            {
                DetachBrain(); 
            }            
        }

        private void DetachBrain()
        {
            mergeBlock.ApplyAction ("OnOff_Off");
        }
        #endregion
        #region POST_SCRIPT
    }    
}
#endregion POST_SCRIPT
