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
    /**
     * reference: https://github.com/tommallama/CSharp-PID
     */
    const double samplePeriod = 1/60;

    double previousOutput = 3.0f;

    double rollup1, rollup2, rollup3, rollup4, rollup5, rollup6, rollup7;
    double output1, output2, output3;
    double error1, error2, error3;

    double proportionalGain;
    double integralGain;
    double derivativeGain;
    double filterCoefficient;

    bool firstTime = true;
    bool stopped = false;
    IMyThrust thruster;
    IMyShipController controller;

    public void Setup()
    {
        Runtime.UpdateFrequency = UpdateFrequency.Update1;

        var controllers = new List<IMyShipController>();
        GridTerminalSystem.GetBlocksOfType(controllers);

        if (controllers.Count  == 0)
        {
            string message = "remote control not found";
            Echo(message);
            throw new Exception(message);
        }
        controller = controllers[0];
        controller.ApplyAction("OnOff_On");

        var thrusterList = new List<IMyThrust>();
        GridTerminalSystem.GetBlocksOfType(thrusterList);

        if (thrusterList.Count == 0)
        {
            string message = "thrusters not found";
            Echo(message);
            throw new Exception(message);
        }

        for (int i = 0; i < thrusterList.Count; i++)
        {
            var current = thrusterList[i];

            if (current.Orientation.ToString().Split(',')[0].Split(':')[1] == "Down")
            {
                thruster = current;
            }
        }

        if (thruster == null)
        {
            string message = "down thruster not found";
            Echo(message);
            throw new Exception(message);
        }
        thruster.Enabled = true;
    }

    public void Main(string arg)
    {
        if (arg == "stop" || stopped == true)
        {
            stopped = true; //prevents queued executions from interfering with a stop
            return;
        }

        if (firstTime)
        {
            firstTime = false;
            Setup();     
        }
        ParseArguments(arg);
        double setPoint = GetSetPoint();
        double elevation = GetElevation();
        PIDThrust(setPoint, elevation);
        Me.TryRun(string.Empty);
    }

    void ParseArguments(string input)
    {
        if (input == string.Empty)
        {
            return;
        }
        var strings = input.Split(',');
        proportionalGain = double.Parse(strings[0]); //proportional gain
        integralGain = double.Parse(strings[0]); //integral gain
        derivativeGain = double.Parse(strings[0]); //derivative gain
        filterCoefficient = double.Parse(strings[0]); //filter coefficient
    }

    double GetElevation()
    {
        double elevation;
        bool outcome1 = controller.TryGetPlanetElevation(MyPlanetElevation.Surface, out elevation);

        if (outcome1 == false)
        {
            string message = "unable to get planet elevation";
            Echo(message);
            throw new Exception(message);
        }
        return elevation;
    }

    double GetSetPoint()
    {
        double speed = controller.GetShipSpeed();

        if (speed <= 10.0f)
        {
            return 3.0f;
        }

        else if (speed <= 50.0f)
        {
            return 5.0f;
        }

        else if (speed <= 100.0f)
        {
            return 7.0f;
        }

        else if (speed <= 150.0f)
        {
            return 9.0f;
        }

        else
        {
            return 11.0f;
        }
    }

    void PIDThrust(double setPoint, double processValue)
    {
       double tao = previousOutput / 
       double P = 1.2 / proportionalGain * 
    }
    #endregion in-game
}
