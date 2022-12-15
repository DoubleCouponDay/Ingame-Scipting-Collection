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
                Vector3D targetLocation = new Vector3D (9, 9, 9);
                Vector3D myLocation = Me.CubeGrid.GetPosition();
                Vector3D difference3D = targetLocation - myLocation;
                double xRotation = Math.Atan2 (difference3D.Z, difference3D.Y);
                double yRotation = Math.Atan2 (difference3D.X, difference3D.Z);
                QuaternionD targetRotation = QuaternionD.CreateFromYawPitchRoll (xRotation, yRotation, 0);
                Quaternion myForwardAngles;
                remoteControl.Orientation.GetQuaternion (out myForwardAngles);
                
                ApplyGyroscopeRotation (myForwardAngles.X, (float) targetRotation.X);
                ApplyGyroscopeRotation (myForwardAngles.Y, (float) targetRotation.Y);

                //QuaternionD matrixToQuat = QuaternionD.CreateFromRotationMatrix (shipsOrientation);

                //printConsole.WritePublicText
                //(
                //    "ships orientation matrix: \n" + 
                //    matrixToQuat.ToString()
                //);
                //string test = "{X:-0.552343986629085 Y:0.709696766479144 Z:0.418826034501225 W:0.125822948609039}";

                //targetsConsole.WritePublicText
                //(
                //    "ships world matrix: \n" +
                //    Me.CubeGrid.WorldMatrix.ToString()
                //);
    
                //printConsole.WritePublicText 
                //(
                //    "SHIPS ORIENTATION: \n" + 
                //    shipsOrientation.M11 + " " + shipsOrientation.M12 + " " + shipsOrientation.M13 + " " + shipsOrientation.M14 + "\n" +
                //    shipsOrientation.M21 + " " + shipsOrientation.M22 + " " + shipsOrientation.M23 + " " + shipsOrientation.M24 + "\n" +
                //    shipsOrientation.M31 + " " + shipsOrientation.M32 + " " + shipsOrientation.M33 + " " + shipsOrientation.M34 + "\n" +
                //    shipsOrientation.M41 + " " + shipsOrientation.M42 + " " + shipsOrientation.M43 + " " + shipsOrientation.M44 + "\n"
                //);
                printConsole.ShowPublicTextOnScreen();
                targetsConsole.ShowPublicTextOnScreen();
            }

            else
            {
                Initialise();
            }
Echo ("main end");
        }

        void 

        void ApplyGyroscopeRotation (AxisType callsAxisType, float comparedNumber)
        {
            switch (callsAxisType)
            {
                case AxisType.X:
                    gyroscope.Pitch = comparedNumber;
                    break;

                case AxisType.Y:
                    gyroscope.Yaw = comparedNumber;
                    break;

                case AxisType.Z:
                    gyroscope.Roll = comparedNumber;
                    break;                
            }
        }

        enum AxisType
        {
            X,
            Y,
            Z,
        }
#endregion in-game
    }
}    
