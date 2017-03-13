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

namespace SingleScript2999876543
{
    public class Program : MyGridProgram
    {
#region in-game
        IMyRemoteControl remote;
        IMyGyro gyroscope;
        IMyTextPanel printConsole;

        public Program()
        {
            remote = GridTerminalSystem.GetBlockWithName ("REMOTE") as IMyRemoteControl;
            gyroscope = GridTerminalSystem.GetBlockWithName ("GYRO") as IMyGyro;
            printConsole = GridTerminalSystem.GetBlockWithName ("CONSOLE") as IMyTextPanel;
        }

        public void Main()
        {
            MatrixD shipsOrient = Me.CubeGrid.WorldMatrix.GetOrientation();
            QuaternionD matrixToQuat = QuaternionD.CreateFromRotationMatrix (shipsOrient); 
            Vector3D shipsPosition = Me.CubeGrid.GetPosition();
            Vector3D targetPosition = new Vector3D (9, 9, 9);
            Vector3D difference = shipsPosition - new Vector3D (9, 9, 9);
            double newXRotation = Math.Acos (shipsPosition * targetPosition);
            Math.Atan2 ();
        }
#endregion in-game
    }
}    
