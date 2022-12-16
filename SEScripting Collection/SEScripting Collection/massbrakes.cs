
using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;

public class MassBrakes : MyGridProgram
{
#region in-game

    public void Main()
    {
        List<IMyTerminalBlock> drivegens = new List<IMyTerminalBlock>();
        GridTerminalSystem.GetBlocksOfType<IMyGravityGenerator>(drivegens);
        List<IMyTerminalBlock> massblocks = new List<IMyTerminalBlock>();
        GridTerminalSystem.GetBlocksOfType<IMyVirtualMass>(massblocks);
        IMyTerminalBlock timer = GridTerminalSystem.GetBlockWithName("mass brakes timer");
        List<IMyTerminalBlock> antenna = new List<IMyTerminalBlock>();				//only for testing 
        GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna>(antenna);       			//only for testing 

        int iCounter = System.Convert.ToInt32(massblocks[1].CustomName); //needed to be int32 instead of int16.

        if (iCounter == 1)
        {
            massblocks[0].SetCustomName("0.0/0.0/0.0"); //give our external variable storage an initial value to prevent error.
            massblocks[1].SetCustomName("2");
        }

        else if (iCounter >= 2 && drivegens != null && massblocks != null)
        {
            double currentX = Math.Round(massblocks[0].GetPosition().X, 4); //take the absolute value of the current dimension, rounded to 4 dp.
            double currentY = Math.Round(massblocks[0].GetPosition().Y, 4);
            double currentZ = Math.Round(massblocks[0].GetPosition().Z, 4);
            string sX = System.Convert.ToString(currentX);    //serialization; 0 prep data for external storage.
            string sY = System.Convert.ToString(currentY);
            string sZ = System.Convert.ToString(currentZ);
            string[] fragments = massblocks[0].CustomName.Split('/');
            double previousX = System.Convert.ToDouble(fragments[0]);  //deserialization; bring back the data.
            double previousY = System.Convert.ToDouble(fragments[1]);
            double previousZ = System.Convert.ToDouble(fragments[2]);
            double deltaX = currentX - previousX;
            double deltaY = currentY - previousY;
            double deltaZ = currentZ - previousZ;
            double distance = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2) + Math.Pow(deltaZ, 2)); //uses pythagorean theorem to find when acceleration gets near to 0;
            massblocks[0].SetCustomName(sX + " / " + sY + " / " + sZ); //sets present coords as previous coords in external variable storage.	
            antenna[0].SetCustomName(sX + " / " + sY + " / " + sZ + " / distance: " + distance);

            if (iCounter == 2)
            {
                massblocks[1].SetCustomName("3");
            }

            else
            {
                PARTTWO(drivegens, massblocks, antenna, iCounter, distance);
            }
        }

        else if (iCounter == 0)
        {
            massblocks[1].SetCustomName("1");
            goto endlooping;
        }

        else
        {
            massblocks[1].SetCustomName("1");
        }
        timer.ApplyAction("TriggerNow");
        endlooping:
        ;
    }

    //I created this method to try avoiding "one byte branch" exceptions. Doesn't work that way but at least everything works despite the exception!
    void PARTTWO(List<IMyTerminalBlock> drivegens, List<IMyTerminalBlock> massblocks, List<IMyTerminalBlock> antenna, int iCounter, double distance)
    {
        if (distance > 0.18 && iCounter == 3)
        {
            massblocks[1].SetCustomName("4");

            for (int a = 0; a < drivegens.Count; a++)
            {
                float gravA = drivegens[a].GetValue<float>("Gravity");

                if (gravA == 9.81f)
                {
                    drivegens[a].ApplyAction("OnOff_On");
                    drivegens[a].SetValue<float>("Gravity", -9.81f);
                }

                else if (gravA == -9.81f)
                {
                    drivegens[a].ApplyAction("OnOff_On");
                    drivegens[a].SetValue<float>("Gravity", 9.81f);
                }
            }
        }

        else if (distance <= 0.18 && distance > 0.0 && iCounter == 4)
        {
            antenna[0].SetCustomName(antenna[0].CustomName + " / disabled.");
            massblocks[1].SetCustomName("3");

            for (int b = 0; b < drivegens.Count; b++)
            {
                float gravB = drivegens[b].GetValue<float>("Gravity");
                if (gravB == 9.81f)
                {
                    drivegens[b].ApplyAction("OnOff_Off");
                    drivegens[b].SetValue<float>("Gravity", -9.81f);
                }

                else if (gravB == -9.81f)
                {
                    drivegens[b].ApplyAction("OnOff_Off");
                    drivegens[b].SetValue<float>("Gravity", 9.81f);
                }
            }
        }

        else if (distance == 0.0)
        {
            antenna[0].SetCustomName("program ended.");
            massblocks[1].SetCustomName("0");

            for (int c = 0; c < massblocks.Count; c++)
            {
                massblocks[c].ApplyAction("OnOff_Off");
            }
        }
        return;
    }
    #endregion in-game
}


