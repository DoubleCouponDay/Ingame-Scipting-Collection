
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using VRage.Game;
using VRage.Library;
using Sandbox.Game.GameSystems;
using System.Text;
using System.Collections.Generic;
using System;

namespace Ingame_Scripting_Collection1
{
    class Program : MyGridProgram
    {
#region in-game
        // This is the name of the assembler you want to control.   

        const string assemblerName = "Assembler"; 
 
        // This is the name of the LCD that will show what the assembler is producing.   
 
        const string outputLCD = "LCD Panel"; 
 
        /************************************   
        Instructions:   
        This script uses the Argument box in the Programmable Block,   
        You set components to be built by putting   
        a ( in front of the component to be made then add a / followed by number to be made then end with )   
        Example:   
        (SteelPlate/10)   
        will build 10 steel plates.   
    
        You can put as many as you like in the argument box.   
        Example:   
        (RadioCommunication/100) (Display/4) (Medical/20)   
        This will build 100 Radios, 4 Displays, and 20 Medical Components.   
    
        This script is case insensitive, which means you don't have to worry about typing the component right.   
        Example:   
        (mOtOR/1) (MOTOR/1)   
        These will both work.   
 
        To clear the assembler queue type the word clear in the argument box. This is also case Insensitive. 
    
        I've also included short names for components.   
        For example, BulletProofGlass can be typed in as Bulletproof or just Bullet.   
        All the components are in the Component List Below.   
    
    
        Component List   
        ==============   
        You can type these any way shown.   
        ==============   
    
        BulletproofGlass / Bulletproof / Bullet     
        Computer     
        Construction     
        Detector     
        Display     
        Explosives     
        Girder     
        GravityGenerator / Gravity   
        InteriorPlate  / Interior   
        LargeTube / Large   
        Medical     
        MetalGrid / Metal   
        Motor     
        PowerCell / Power   
        RadioCommunication / Radio   
        Reactor     
        SmallTube / Small   
        SolarCell / Solar   
        SteelPlate / Steel   
        Thrust     
        Superconductor / Super  
    
        ************************************/ 
 
        private IMyProductionBlock assembler; 
        private StringBuilder dynamicContent = new StringBuilder(); 
 
        public Program() 
        { 
            InitialiseAssembler(); 
        } 

        public bool InitialiseAssembler() 
        { 
            assembler = GridTerminalSystem.GetBlockWithName(assemblerName) as IMyProductionBlock; 
 
            if (assembler == null) 
            { 
                return false; 
            } 
 
            return true; 
        } 
 
        public void Main(string argument) 
        { 
            Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); 
            values["computer"] = "ComputerComponent"; 
            values["motor"] = "MotorComponent"; 
            values["steelplate"] = "SteelPlate"; 
            values["steel"] = "SteelPlate"; 
            values["construction"] = "ConstructionComponent"; 
            values["girder"] = "GirderComponent"; 
            values["metalgrid"] = "MetalGrid"; 
            values["metal"] = "MetalGrid"; 
            values["interiorplate"] = "InteriorPlate"; 
            values["interior"] = "InteriorPlate"; 
            values["smalltube"] = "SmallTube"; 
            values["small"] = "SmallTube"; 
            values["largetube"] = "LargeTube"; 
            values["large"] = "LargeTube"; 
            values["display"] = "Display"; 
            values["bulletproofglass"] = "BulletproofGlass"; 
            values["bulletproof"] = "BulletproofGlass"; 
            values["bullet"] = "BulletproofGlass"; 
            values["reactor"] = "ReactorComponent"; 
            values["thrust"] = "ThrustComponent"; 
            values["gravitygenerator"] = "GravityGeneratorComponent"; 
            values["gravity"] = "GravityGeneratorComponent"; 
            values["medical"] = "MedicalComponent"; 
            values["radiocommunication"] = "RadioCommunicationComponent"; 
            values["radio"] = "RadioCommunicationComponent"; 
            values["detector"] = "DetectorComponent"; 
            values["explosives"] = "ExplosivesComponent"; 
            values["solarcell"] = "SolarCell"; 
            values["solar"] = "SolarCell"; 
            values["powercell"] = "PowerCell"; 
            values["power"] = "PowerCell"; 
            values["superconductor"] = "Superconductor"; 
            values["super"] = "Superconductor"; 
 
            if (assembler == null) 
            { 
                if (!InitialiseAssembler()) 
                { 
                    WriteToTextPanel(assemblerName + " not installed"); 
                    return; 
                } 
            } 
            string arg = argument.ToLower(); 
 
            List<string> groups = new List<string>(); 
 
            int start = 0; 
            while ((start = arg.IndexOf('(', start)) != -1) 
            { 
                int end = (start >= 0) ? arg.IndexOf(')', start) : -1; 
                string result = (end >= 0) ? arg.Substring(start + 1, end - start - 1) : ""; 
                groups.Add(result); 
 
                start++; 
            } 
 
            dynamicContent.Clear(); 
            foreach (var selectedGroup in groups) 
            { 
                List<string> args = new List<string>(selectedGroup.Split('/')); 
 
                if (values.ContainsKey(args[0])) 
                { 
                    string item = values[args[0]]; 
                    string count = args[1]; 
 
                    if (count == "" || Convert.ToDouble(count) <= 0) 
                    { 
                        Echo("Item: " + item + "\nAmount: " + "Wrong or No number input\n"); 
                        return; 
                    } 
                    MyDefinitionId objectIdToAdd = new MyDefinitionId(); 
                    if (MyDefinitionId.TryParse("MyObjectBuilder_BlueprintDefinition/" + item, out objectIdToAdd)) 
                    { 
                        Echo("Item: " + item + "\nAmount: " + Convert.ToDouble(count) + "\n"); 
                        assembler.AddQueueItem(objectIdToAdd, Convert.ToDouble(count)); 
                        dynamicContent.AppendLine("Item: " + item + "\nAmount: " + Convert.ToDouble(count) + "\n"); 
                    } 
                } 
                else 
                { 
                    Echo("Argument Missing: Check Name \n"); 
                } 
            } 
            if (arg == "clear") 
            { 
                dynamicContent.Clear(); 
                assembler.ClearQueue(); 
                Echo("Cleared Assembler Queue."); 
                dynamicContent.AppendLine("Cleared Assembler Queue."); 
            } 
 
            WriteToTextPanel(dynamicContent.ToString()); 
        } 
 
        public void WriteToTextPanel(string input) 
        { 
            List<IMyTerminalBlock> lcds = new List<IMyTerminalBlock>(); 
            GridTerminalSystem.SearchBlocksOfName(outputLCD, lcds, b => b.CubeGrid == Me.CubeGrid); 
 
            if (lcds.Count == 0) 
            { 
                Echo("\nNo LCD with name '" + outputLCD + "' found.\n" + "Check the name."); 
            } 
            foreach (IMyTextPanel lcd in lcds) 
            { 
                lcd.WritePublicText(input); 
                lcd.ShowPublicTextOnScreen(); 
            } 
        }
#endregion in-game
    }
}
