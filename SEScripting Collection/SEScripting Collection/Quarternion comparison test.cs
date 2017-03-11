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
using System.Linq;

namespace SingleScript2777
{
    public class Program : MyGridProgram
    {
#region in-game
        IMyRemoteControl remoteControl;
        IMyTextPanel printConsole;
        IMyTextPanel targetsConsole;
        IMyGyro gyroscope;

        IMyRadioAntenna antenna;
        List <IMyRadioAntenna> allAntennas = new List<IMyRadioAntenna>();

        object[] nullChecker;

        Vector3D testLocation = new Vector3D (9, 9, 9);

        bool compiled;

        void Initialise()
        {
Echo ("Initialise start");
            int nullCount = 0;

            remoteControl = GridTerminalSystem.GetBlockWithName ("REMOTE") as IMyRemoteControl;
            printConsole = GridTerminalSystem.GetBlockWithName ("CONSOLE") as IMyTextPanel;
            targetsConsole = GridTerminalSystem.GetBlockWithName ("TARGET CONSOLE") as IMyTextPanel;
            gyroscope = GridTerminalSystem.GetBlockWithName ("GYROSCOPE") as IMyGyro;
            GridTerminalSystem.GetBlocksOfType (allAntennas);
            antenna = allAntennas.FirstOrDefault();

            nullChecker = new object[]
            {
                remoteControl,
                printConsole,
                targetsConsole,
                gyroscope,
                antenna,
            };

            for (int i = 0; i < nullChecker.Length; i++)
            {
                if (nullChecker[i] == null)
                {
                    nullCount++;
                }
            }

            if (nullCount != 0)
            {
                compiled = true;
            }

            else
            {
                Echo ("NO BLOCK FOUND: " + nullCount.ToString());
            }
Echo ("initialise end");
        }

        //http://xboxforums.create.msdn.com/forums/t/10228.aspx
        public void Main()
        {
Echo ("main start");
            if (compiled)
            {
                Vector3D myPosition = Me.CubeGrid.GetPosition();
                Vector3D difference = myPosition - testLocation;
                Vector3D rightSomething = Vector3D.Cross (Vector3D.UnitY, myPosition);
                Vector3D backwards = Vector3D.Normalize (rightSomething);
                Vector3D up = Vector3D.Cross (backwards, rightSomething);
                Matrix rotatedTowardsTarget = new Matrix ((float) rightSomething.X, (float) rightSomething.Y, (float) rightSomething.Z, 0, (float) up.X, (float) up.Y, (float) up.Z, 0, (float) backwards.X, (float) backwards.Y, (float) backwards.Z, 0, 0, 0, 0, 1);
                
                printConsole.WritePublicText 
                (
                    "MY MATRIX: \n" + 
                    rotatedTowardsTarget.M11 + " " + rotatedTowardsTarget.M12 + " " + rotatedTowardsTarget.M13 + " " + rotatedTowardsTarget.M14 + "\n" +
                    rotatedTowardsTarget.M21 + " " + rotatedTowardsTarget.M22 + " " + rotatedTowardsTarget.M23 + " " + rotatedTowardsTarget.M24 + "\n" +
                    rotatedTowardsTarget.M31 + " " + rotatedTowardsTarget.M32 + " " + rotatedTowardsTarget.M33 + " " + rotatedTowardsTarget.M34 + "\n" +
                    rotatedTowardsTarget.M41 + " " + rotatedTowardsTarget.M42 + " " + rotatedTowardsTarget.M43 + " " + rotatedTowardsTarget.M44 + "\n"
                );

                targetsConsole.WritePublicText
                (
                    "TARGETS MATRIX: \n" +                    
                    remoteControl.WorldMatrix.M11 + " " + remoteControl.WorldMatrix.M12 + " " + remoteControl.WorldMatrix.M13 + " " + remoteControl.WorldMatrix.M14 + "\n" +
                    remoteControl.WorldMatrix.M21 + " " + remoteControl.WorldMatrix.M22 + " " + remoteControl.WorldMatrix.M23 + " " + remoteControl.WorldMatrix.M24 + "\n" +
                    remoteControl.WorldMatrix.M31 + " " + remoteControl.WorldMatrix.M32 + " " + remoteControl.WorldMatrix.M33 + " " + remoteControl.WorldMatrix.M34 + "\n" +
                    remoteControl.WorldMatrix.M41 + " " + remoteControl.WorldMatrix.M42 + " " + remoteControl.WorldMatrix.M43 + " " + remoteControl.WorldMatrix.M44 + "\n"
                );
                printConsole.ShowPublicTextOnScreen();
                targetsConsole.ShowPublicTextOnScreen();
            }

            else
            {
                Initialise();
            }
Echo ("main end");
        }
#endregion in-game
    }
}    
