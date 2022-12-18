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
using System.Diagnostics.SymbolStore;

public class Gonzalez : MyGridProgram
{

    #region in-game
    /**
     * reference: https://github.com/tommallama/CSharp-PID
     */
    const double samplePeriod = 1/60;
    const double minimumHeight = 5.0f;

    double error1, error2, error3, error4, error5;

    double proportionalGain;
    double integralGain;
    double derivativeGain;

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
        integralGain = double.Parse(strings[1]); //integral gain
        derivativeGain = double.Parse(strings[2]); //derivative gain
    }

    double GetElevation() {
        bool outcome1 = controller.TryGetPlanetElevation(MyPlanetElevation.Surface, out double elevation);

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
            return minimumHeight;
        }

        else if (speed <= 50.0f)
        {
            return minimumHeight + 2;
        }

        else if (speed <= 100.0f)
        {
            return minimumHeight + 2 * 2;
        }

        else if (speed <= 150.0f)
        {
            return minimumHeight + 2 * 3;
        }

        else
        {
            return minimumHeight + 2 * 4;
        }
    }

    void PIDThrust(double setPoint, double currentAltitude)
    {
        double error = currentAltitude - setPoint;
        //set history back by 1
        error5 = error4;
        error4 = error3;
        error3 = error2;
        error2 = error1;
        error1 = error;

        double P = proportionalGain * error;

        //trapezoidal rule to find the integral component
        double Isum = samplePeriod / 2;

        for(int i = 0; i < 5; i++) {
            Isum += 
        }

        double I = integralGain * Isum;
    }
    #endregion in-game
}
