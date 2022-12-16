using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using VRage.Game;
using System.Collections.ObjectModel;
using VRage.Game.ModAPI.Ingame;

public class Gonzalez : MyGridProgram
{
    #region in-game

    bool firstTime = true;
    IMyCameraBlock top;
    IMyCameraBlock bot;

    public void Setup()
    {
        Runtime.UpdateFrequency = UpdateFrequency.Update1;
        var cameraList = new List<IMyCameraBlock>();
        GridTerminalSystem.GetBlocksOfType<IMyCameraBlock>(cameraList);

        if (true)
        {

        }
        var firstOne = cameraList[0];
        var secondOne = cameraList[1];
    }

    public void Main()
    {
        if (firstTime)
        {
            firstTime = false;
            bool outcome = Setup();

            if(outcome == false)
            {
                Echo("Setup failed");
                return;
            }                
        }
    }

    void BalanceGyros()
    {

    }

    void 

    public void Save()
    {

    }
    #endregion in-game
}
