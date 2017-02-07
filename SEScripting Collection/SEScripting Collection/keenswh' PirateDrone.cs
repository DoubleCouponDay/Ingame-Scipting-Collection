
using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

namespace Ingame_Scripting_Collection3
{
    class Program : MyGridProgram
    {
#region in-game
        List<IMyTerminalBlock> list = new List<IMyTerminalBlock>();  
        IMyTimerBlock timer;
         
        IMyWarhead bomb;
        IMySensorBlock sensor;
        IMyShipMergeBlock mergeBlock;

        public Program()
        {
            timer = GridTerminalSystem.GetBlockWithName ("TIMER") as IMyTimerBlock;

            bomb = GridTerminalSystem.GetBlockWithName ("BOMB") as IMyWarhead;
            sensor = GridTerminalSystem.GetBlockWithName ("SENSOR") as IMySensorBlock;
            mergeBlock = GridTerminalSystem.GetBlockWithName ("MERGE_BLOCK") as IMyShipMergeBlock;
        }

        public void Main (string argument) 
        {            
            if (sensor != null)
            {
                if (sensor.IsWorking && mergeBlock.Enabled) //sensor must turn off the merge block when it detects a player.
                {
                    bomb.SetValueFloat ("DetonationTime", 3.0F);
                    bomb.SetValueBool ("Safety", false);
                    bomb.ApplyAction ("StartCountdown");
                }

                else
                {                    
                    mergeBlock.ApplyAction ("OnOff_Off");
                }
            }

            else
            {
                mergeBlock.ApplyAction ("OnOff_Off");
            }

            Vector3D origin = new Vector3D (0, 0, 0); 

            if (this.Storage == null || this.Storage == "") 
            { 
                origin = Me.GetPosition(); 
                this.Storage = origin.ToString(); 
            } 

            else 
            { 
                Vector3D.TryParse (this.Storage, out origin); 
            }  
            GridTerminalSystem.GetBlocksOfType<IMyRemoteControl> (list); 

            if (list.Count > 0) 
            { 
                var remote = list[0] as IMyRemoteControl; 
                remote.ClearWaypoints(); 
                Vector3D player = new Vector3D (0, 0, 0); 
                bool success = remote.GetNearestPlayer (out player); 

                if (success) 
                { 
                    bool gotoOrigin = false; 
                    GridTerminalSystem.GetBlocksOfType<IMyUserControllableGun> (list); 

                    if (list.Count == 0) 
                    { 
                        gotoOrigin = true; 
                    } 

                    else 
                    { 
                        bool hasUsableGun = false; 

                        for (int i = 0; i < list.Count; ++i)
                        {
                            var weapon = list[i];
                            if (!weapon.IsFunctional) continue;
                            if (weapon.HasInventory() && !weapon.GetInventory(0).IsItemAt(0)) continue;

                            hasUsableGun = true;
                        }

                        if (!hasUsableGun)
                        {
                            gotoOrigin = true;
                        }
                    }

                    if (Vector3D.DistanceSquared(player, origin) > 20000 * 20000) 
                    { 
                        gotoOrigin = true; 
                    } 
 
                    if (gotoOrigin) 
                    { 
                        remote.AddWaypoint(origin, "Origin"); 
                    } 

                    else 
                    { 
                        remote.AddWaypoint(player + remote.GetTotalGravity() * -2f, "Player"); //20 metres above player
                    } 
                    remote.SetAutoPilotEnabled (true); 
                } 
            } 
            timer.ApplyAction ("Start");
        }
        #endregion in-game
    }    
}