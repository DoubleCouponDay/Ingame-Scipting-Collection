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
    const double samplePeriod = 1.0f/60.0f;
    const double minimumHeight = 10.0f;

    double[] errors = new double[5];

    double proportionalGain = 1;
    double integralGain = 1;
    double derivativeGain = 10;

    bool firstTime = true;
    bool stopped = false;
    IMyThrust thruster;
    IMyRemoteControl controller;

    public void Setup()
    {
        Runtime.UpdateFrequency = UpdateFrequency.Update1;

        var controllers = new List<IMyRemoteControl>();
        GridTerminalSystem.GetBlocksOfType(controllers);

        if (controllers.Count == 0)
        {
            string message = "remote control not found";
            Echo(message);
            throw new Exception(message);
        }
        controller = controllers[0];
        controller.DampenersOverride = false;
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
        if (firstTime)
        {
            firstTime = false;
            Setup();
        }

        if (arg != "stop" && arg != string.Empty)
        {
            stopped = false;
            controller.DampenersOverride = false;
        }

        else if (arg == "stop" || stopped == true)
        {
            stopped = true;

            if (thruster != null)
            {
                thruster.ThrustOverridePercentage = 0.0f;
                controller.DampenersOverride = true;
            }
            return;
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
        double elevation;
        bool outcome1 = controller.TryGetPlanetElevation(MyPlanetElevation.Surface, out elevation);

        if (outcome1 == false)
        {
            string message = "unable to get planet elevation";
            Echo(message);
            throw new Exception(message);
        }
        Echo($"elevation: {elevation}");
        return elevation;
    }

    double GetSetPoint()
    {
        double speed = controller.GetShipSpeed();
        double output;

        if (speed <= 10.0f)
        {
            output = minimumHeight;
        }

        else if (speed <= 50.0f)
        {
            output = minimumHeight + 2;
        }

        else if (speed <= 100.0f)
        {
            output = minimumHeight + 2 * 2;
        }

        else if (speed <= 150.0f)
        {
            output = minimumHeight + 2 * 3;
        }

        else
        {
            output = minimumHeight + 2 * 4;
        }
        Echo($"setpoint: {output}");
        return output;
    }

    void PIDThrust(double setPoint, double currentAltitude)
    {
        double error = setPoint - currentAltitude;

        errors[4] = errors[3];
        errors[3] = errors[2];
        errors[2] = errors[1];
        errors[1] = errors[0];
        errors[0] = error;
        
        double P = proportionalGain * error;

        //trapezoidal rule to find the integral component
        double I = samplePeriod / 2;
        I += errors[4] + errors[0];

        for (int i = 1; i < errors.Length - 1; i++)
        {
            I += 2 * integralGain * errors[i] * samplePeriod;
        }
        double D = derivativeGain * (error - errors[1]) / samplePeriod;

        Echo($"P: {P}");
        Echo($"I: {I}");
        Echo($"D: {D}");

        thruster.ThrustOverridePercentage = (float)(P + I + D);
    }
    #endregion in-game
}
