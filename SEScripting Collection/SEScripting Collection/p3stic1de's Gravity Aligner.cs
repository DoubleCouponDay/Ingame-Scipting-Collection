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

namespace stuff555555
{
    class Program
    {
// public domain code by Sean L. Palmer

// Planetary vehicle guidance assistance system.
// A gyro gravity orientation aligner.
// Orients a ship so that its specified remote control's Down axis is aligned with local gravity force.
// to within a specified angle tolerance
// In other words, keeps hovercraft from flipping over, aligned with the ground.
// The main ship inertial dampers handle keeping it afloat!
// Some folks use it for satellites, some use it as mecha balance assistance.

// BLOCKS NEEDED:
//   a programmable block, with this script
//   a timer block, setup actions to Run the programmable block with blank argument
//   a remote control or cockpit facing the proper way (down direction is most important)
//   some gyros, any orientation
//   a text panel (optional) for diagnostics
// Ownership of all the blocks should be the same, to ensure they can intercommunicate.
// Make a group named same as GroupName, containing all those blocks.
// Now Run the script with Argument "go" somehow; 
// If you want to stop the aligner, Run with Argument "stop" to gracefully shut off all the gyro/thruster overrides
// I like to set up the ship cockpit toolbar with actions to stop, go the aligner

const string GroupName = "Aligner"; // name of group in terminal containing cockpit, gyros, text panel
const string ScriptTitle = "Gravity Aligner\nby p3st|cIdE\n";

// SCANNER 
 
public static class Scanner 
{ 
#region fill from ctor 
    public static IMyProgrammableBlock Me; 
    public static IMyGridTerminalSystem Terminal; 
    public static string GroupName; 
    public static TimeSpan RescanPeriod = Time.s(5); // damage, building can affect available blocks
#endregion 
    public static IMyBlockGroup Group; 
    public static List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>(); 
    public static bool FromCtor { get { return first; } } 
 
    public static bool Update() 
    { 
        bool success = true; 
        if (DateTime.Now >= RescanTime) try { 
            success = Rescan(); 
        } catch (Exception e) { throw new Exception("Scan(" + first + "):\n" + e.Message); } 
        if (first) { Module.Load(); first = false; } 
        return success; 
    } 
 
    public static void GetBlocksOfType<T>(List<T> list) where T : class 
    { 
        foreach (var t in Blocks) { var tt = t as T; if (tt != null) list.Add(tt); }
    }

    public static float Volume(IMyCubeBlock block) // in L 
    { 
        int nb = (block.Max - block.Min + 1).Volume(); //Dot(ref Vector3I.One); // total blocks in bounding box 
        float e = block.CubeGrid.GridSize * .1f; // edge length in dm 
        e *= e*e; // cubed for size in L of 1 cube 
        return e * nb; 
    }

    static bool Rescan() 
    { 
        bool success = true; 
        Log.scan.Clear(); 
        Log.diagnostic.Clear(); 
        Schedule(); 
        FindBlocks(); 
        if (first) 
          try { 
            bool ok = Check(); // NOT fatal 
          } catch (Exception e) { throw new Exception("Scan.Check:\n" + e.Message); } 
        foreach (var m in Module.All) 
          try { 
            if (!m.Scan()) 
            { 
                Log.scan.AppendLine(m.Name + " scan fail!"); 
                success = false;  
            } // persist to show all failures in log 
          } catch (Exception e) { throw new Exception("Scan:" + m.Name + '\n' + e.Message); } 
        if (!success) Log.scan.AppendLine("Blocks missing!\nCheck ownership consistency!"); 
        Log.Echo(Log.scan.ToString()); 
        Log.Echo(Log.diagnostic.ToString()); 
        return success; 
    }

    static void FindBlocks() 
    {
        if (Group == null && GroupName != null) 
        { 
            Group = Terminal.GetBlockGroupWithName(GroupName);
            if (Group == null) 
                Log.scan.AppendLine("No group called '" + GroupName + "' found!"); 
        }
        Blocks.Clear();
        if (Group != null) Group.GetBlocks(Blocks);
        else Terminal.GetBlocks(Blocks);
    }

    // First Run after world load or paste during mass initialization of all blocks in the GridTerminalSystem,
    // when blocks may not all yet be available!  May cause strange errors about missing blocks.
    // Otherwise we'd like to turn certain blocks on.

    static bool Check() 
    { 
        bool good = true;
        foreach (var b in Blocks)
        { 
            if (b.CubeGrid.IsStatic)
                 if (b is IMyThrust || b is IMyGyro || b is IMyShipController) 
                     Log.scan.AppendLine("'" + b.CustomName+ "' is on station, can't use!"); // will also report !IsWorking 
            if (!b.IsFunctional) // don't automatically re-enable blocks that may require manually disabled if things go kaput
                if (b is IMyTimerBlock || b is IMyTextPanel)
                    (b as IMyFunctionalBlock).Enabled = true;
            // in ctor, ignore IsWorking status because the rest of the ship may not be initialized yet.
            if (!(b is IMyThrust) && !b.IsWorking) // broken/off/unpowered blocks don't help
            { // but ALL blocks report !IsWorking during first ctor run...  Thrusts we toggle all the time, they can be off.
                Log.scan.AppendLine("'" + b.CustomName + "' is not working!"); //good = false; // not fatal.
            }
        }
        return good;
    } 
 
    static void Schedule() 
    { // initial scan likely bad, so reschedule for next Run, log to PB console 
        if (first) { RescanTime = DateTime.Now; Log.Echo(Log.scan.ToString()); first = false; } 
        else RescanTime = DateTime.Now + RescanPeriod; 
    } 
 
    static DateTime RescanTime = DateTime.Now;
    static bool first = true; // called from Program ctor? 
} 

// MODULE
// Aids static classes coordination.
// Eases development in many ways.
public class Module 
{ 
    public string Name = "?"; 
    // Poor man's static class interface! 
    // These delegates may not be null, but they all have sensible default implementations. 
    public Func<bool> Scan = () => true; // skip scan and report success 
    public Func<string[], bool> Parse = s => false; // parse one pre-tokenized command, unhandled 
    public Action Tick = () => {}; // called once per frame; use Time module! 
    public Func<string[]> Save = () => null; // save no state into array of commands 
 
    // Structures script execution. 
    // Add all Modules during Program constructor. 
    public static List<Module> All = new List<Module>(); 
 
    public static bool Commands(string commands, bool savechanges = false) 
    { 
        if (commands == null 
         || (commands = commands.Trim()).Length == 0) 
            return true;
        try {
            bool allhandled = true;
            bool somehandled = false;
            foreach (var cmd in commands.Split(Utility.LineSeps)) 
            {
                var c = cmd.Trim();
                if (c.Length == 0 // blank lines / in-between '\r' and '\n'
                 || c.StartsWith("//")) // comments 
                    continue; // skip
                if (c == "save") Store();
                else if (c == "load") Load();
                else 
                {
                    string[] tok = c.Split(Utility.AssignSeps);
                    if (tok.Length == 0 || tok[0].Length == 0) continue; // skip blank-ish lines
                    tok[0] = tok[0].ToLowerInvariant(); // don't require of all modules 
                    bool handled = false; 
                    // allow multiple modules to handle the same command, 
                    // contributing functionality (e.g. "stop" overload) 
                    foreach (var m in All) 
                        if (m.Parse != null && m.Parse(tok)) 
                            handled = true; 
                    Log.Echo("'" + c + "': " + (handled ? "ok" : "error")); 
                    if (!handled) allhandled = false; 
                    else somehandled = true; 
                } 
            } 
            if (somehandled && savechanges) Store(); // write back to CustomData 
            return allhandled; 
        } catch (Exception e) { throw new Exception("Module.Load:\n" + e.Message); }
    }
    
    public static bool Load() 
    {
        return Scanner.Me == null ? false : Commands(Scanner.Me.CustomData); 
    }

    public static void Update(IMyGridProgramRuntimeInfo runtime) 
    { 
        if (!Time.Run(runtime)) return; 
        Scanner.Update();
        foreach (var m in All) 
          try { 
            m.Tick(); 
          } catch (Exception e) { throw new Exception("Module.Tick:\n" + m.Name + '\n' + e.Message); } 
    } 
 
    public static string Store() // serialize to string, save to Me.CustomData
    {
        List<string> saved = new List<string>();
        string result = "// if edited manually, reload with command 'load'\n";
        try {
            foreach (var s in All) 
            { 
                var c = s.Save();
                if (c != null) saved.AddArray(c); 
            }
            result += string.Join("\n", saved);
        } catch (Exception e) { throw new Exception("Module.Store:\n" + e.Message + "\nSaving failed!"); } 
        if (Scanner.Me != null) 
            Scanner.Me.CustomData = result; 
        return result; 
    } 
} 
 
// TIME
// timer / update rate management
public static class Time 
{
    public static Module GetModule() 
    { 
        Chart.Title["time"] = TimeChart; 
        Chart.Title["update"] = UpdateChart; 
        var m = new Module(); 
        m.Name = "Time"; 
        m.Scan = Rescan; 
        m.Parse = Parse; 
        m.Save = Save; 
        return m; 
    } 
 
    public static bool running { get { return _running; } } 
    public static bool proceed { get { return _dt.Ticks != 0; } } 
    public static IMyGridProgramRuntimeInfo runtime; // Note: dt != runtime.ElapsedTime
    public static TimeSpan dt { get { return _dt; } private set { _dt = value; } } // delta time since last *full* Run (where we weren't waiting for the update period)
    public static float dts { get { return _dt.Ticks * 1e-7f; } } // _dts; } } //(float)dt.TotalSeconds; } } // delta time in seconds
    public static int dtms { get { return (int)_dt.Ticks / 10000; } }
    public static int ams { get { return (int)DateTime.Now.Ticks / 10000; } } // animated, wraps about 4x/hour; for cosmetic spinners.
    public static long gms { get { return (DateTime.Now - timeStarted).Ticks / 10000; } } // reset each session, otherwise stable increasing.

    public static Chart TimeChart = new Chart();
    public static Chart UpdateChart = new Chart();

    static bool ScanTimers() 
    { 
        Scanner.GetBlocksOfType(Timers); 
        // hopefully one is set to Run this programmable block,  
        // so we can run ourselves again next frame,
        // but there's no way to automate or check that.
        foreach (var t in Timers) InitTimer(t);
        bool good = CanGo; 
        if (!good) Log.scan.AppendLine("No timers in group!\nCannot run!");
        return good;
    } 
 
    static int nticks; 
 
    // All we need is the timer to have Setup Actions so it 
    // Runs the programmable block with default argument.
    // Script manages the rest, but can't yet automate that.
    // pass in info obtained from MyGridProgram.Runtime
    public static bool Run(IMyGridProgramRuntimeInfo runtime) // updates dt, variable timestep, returns whether rest of Modules need ticked
    { 
        TimeChart.Param = UpdateChart.Param = 1.4f; // 3 lines
        UpdateChart.Text = "Elapsed: " + runtime.TimeSinceLastRun.TotalMilliseconds.ToString("f2") + " ms";
        UpdateChart.Text += "\nExec: " + runtime.LastRunTimeMs.ToString("f2") + " ms";
        if (!running) TimeChart.Text = "Stopped";
        var now = DateTime.Now;
        UpdateChart.Text += "\nTick: " + (now - lastRun).TotalMilliseconds.ToString("f2") + " ms"; // true update time
        UpdateChart.Text += "\nTicks: " + ++nticks;
        lastRun = now;
        _dt = now - oldTime; // measure delta
        if (!_running || _dt < UpdatePeriod) _dt = new TimeSpan(); // report 0 elapsed time until full period passed
        else oldTime = now; // advance
        _dts = (float)dt.TotalSeconds; // just a multiply long by double constant
        Retrigger();
        if (running) 
        {
            TimeChart.Text = "Running";
            TimeChart.Text += "\nElapsed: " + dtms.ToString("f1") + " ms";
            TimeChart.Text += '\n';
            if (_dt.Ticks != 0) TimeChart.Text += "(" + Pretty._(1/dts) + " Hz)";
            TimeChart.Text += "\nUpdate: " + Pretty._(UpdateFrequency) + " Hz"; 
        }
        if (_dt.Ticks != 0) nticks = 0;
        return proceed;
    }

    static ITerminalAction timerTriggerNow; // cache very frequently used property

    static void InitTimer(IMyTimerBlock timer)
    {
        if (timerTriggerNow == null)
            timerTriggerNow = timer.GetActionWithName("TriggerNow"); // string lookup is the slow part, only at startup
        // minimum possible period works best with stopping script cleanly 
        timer.SetValueFloat("TriggerDelay", 1f);
    }

    static bool TriggerTimers() // request to be Run again later, next frame
    {
        if (!_running) return false;
        if (Timers.Count == 0 || timerTriggerNow == null) 
        { 
            Log.Output("Cannot retrigger."); 
            return false; 
        }
        foreach (var t in Timers)
            timerTriggerNow.Apply(t);
        return true;
    }

    static void GoTimers() // begin actively controlling the ship
    {
        if (DateTime.Now < stopUntil) return; // do not allow restarting during the StopFlushPeriod
        _running = true;
        oldTime = DateTime.Now - TimeSpan.FromTicks(1);
        foreach (var timer in Timers)
            timer.ApplyAction("Start"); 
        Retrigger(); 
    }
    
    static void StopTimers()
    {
        stopUntil = DateTime.Now + StopFlushPeriod;
        _running = false; 
        foreach (var timer in Timers)
        {
            timer.ApplyAction("Stop"); 
            timer.Enabled = false; // clear out any pending start.  Timers are problematic.
            timer.Enabled = true;
        } 
    }

    const int DefaultUpdateFrequency = 20; //10; // more is not always better
    const double freqBias = .9; // round to not skip updates due to tiny imprecisions
    public static TimeSpan UpdatePeriod = s(1.0 / (DefaultUpdateFrequency + freqBias));
    public static double UpdateFrequency { get { return 1 / UpdatePeriod.TotalSeconds - freqBias; } }
    static TimeSpan StopFlushPeriod = s(2);
    static readonly TimeSpan tick1 = TimeSpan.FromTicks(1);
    static DateTime oldTime = DateTime.Now - tick1;
    static DateTime stopUntil = DateTime.Now - tick1;
    static DateTime lastRun = DateTime.Now - tick1; 
    static readonly DateTime timeStarted = DateTime.Now; // last Recompile or world reload
    static TimeSpan _dt;
    static float _dts; // delta time in seconds

    static List<IMyTimerBlock> Timers  = new List<IMyTimerBlock>();

    static bool _running = false; 
 
    static string[] Save() 
    { 
        return new string[] { 
            _running ? "go" : "stop", 
            "updatefrequency=" + Pretty._(Math.Round(UpdateFrequency)) 
        }; 
    }

    static bool Parse(string[] tokens) 
    { 
        if (tokens.Length == 1) 
        { 
            string varn = tokens[0]; 
            if (varn == "go") Go(); 
            else if (varn == "stop") Stop(); 
            else return false; 
        } else { 
            double val; 
            if (!double.TryParse(tokens[1], out val)) return false; 
            string varn = tokens[0]; 
            if (varn == "updatefrequency" && val >= 1 && val <= 60.5) UpdatePeriod = Frequency(MathHelper.Clamp(val, 1.0, 60.0) + freqBias); 
            else return false; 
        } 
        return true; 
    } 

    public static bool CanGo { get { return Timers.Count > 0; } }
    public static bool Retrigger() { return TriggerTimers(); } // request to be Run again later, next frame    
    public static void Go() { GoTimers(); }
    public static void Stop() { StopTimers(); }
    public static bool Rescan() { return ScanTimers(); } 
 
    // TimeSpan.FromMilliseconds and .FromSeconds both mysteriously round to ms due to internal Interval helper function bug 
    public static TimeSpan ms(double n) => TimeSpan.FromTicks((long)(1e4 * n));
    public static TimeSpan  s(double n) => TimeSpan.FromTicks((long)(1e7 * n));
    public static TimeSpan Frequency(double hz) => TimeSpan.FromTicks((long)(1e7 / hz));
} 

// PID CONTROLLER

// to use such a controller for higher dimensions,
// use a combination of 1D PIDs

public class PIDControllerFloat
{
    public float Kp = 1, Ki = 1, Kd; // some people factor out Kp so Ki and Kd are proportions of it.
    public float Kd2; // nonstandard extension: using partially-squared derivative of error.  REALLY helps control overshoot by limiting how much it can be misaligned.
    public float Kp2; // nonstandard extension: use partially-squared error.  Could help with controlling stuff in SE because it should work great with its crappy spin-up behavior.
    public float integralReduction = .001f;
    private float prevErr;
    private float integral;
    private float target;

    public float Error { get { return prevErr; } }
    public float Integral { get { return integral; } set { integral = value; } } // client may wish to reset the integral occasionally

    public void SetTarget(float s) //, double resetDistance) 
    {
        var targetDelta = s - target;
        prevErr += targetDelta; // pretend like we were already at the old setpoint to avoid a momentary hiccup
        // since we fixed prevErr, integral won't get bashed any harder now; it may take some time to unwind though.
        //if (mag(targetDelta) > resetDistance)
        //	integral = 0; // abandon integral tracking if too far away from old setpoint.  Small changes can keep the integral error.
        target = s; 
    }
    // t is delta time, likely in seconds
    // v is current value of thing we're controlling
    // target is current target value, set point
    public float Advance(float t, float v) // returns the new control value to set to
    {
        var error = target - v;
        integral *= (float)Math.Exp(-integralReduction * t); // reduces slightly over time
        integral += error * t; // accumulate error
        var err2 = error * Math.Abs(error); // sqrsgn
        var derivative = (error - prevErr) / t; // not stored
        var deriv2 = derivative * Math.Abs(derivative);
        prevErr = error; // maintain error state for next run
        return Kp2 * err2
             + Kp * error
             + Ki * integral
             + Kd * derivative
             + Kd2 * deriv2; // sum up
    }
    // https://en.wikipedia.org/wiki/Braking_distance
    public static float MaxVelocityForDistanceStoprate(float d, float r) 
    { 
        if (d < 0) throw new ArgumentException("must be >= 0", "d"); 
        if (r <= 0) return 0; 
        return (float)Math.Sqrt(2 * r * d); 
    }
}

// UTILITY

public static class Utility 
{
    public static float  Sqr(float  x) { return x*x; }
    public static double Sqr(double x) { return x*x; }
    public static float  ClampAbs(float  x, float  lim) { return MathHelper.Clamp(x, -lim, lim); }
    public static double ClampAbs(double x, double lim) { return MathHelper.Clamp(x, -lim, lim); }

    public static float Frac(double x) { x -= (int)x; if (x < 0) x += 1f; return (float)x; }
    public static float Wiggle(float x) { return (2f - Math.Abs(x)) * x; }

    // simple LFOs, phase in double precision, output 0 to 1
    public static bool Square(double w) { w = Frac(w + .25); return w >= .5f; } 
    public static float Saw(double w) { return Frac(w); } 
    public static float Tri(double w) { return Math.Abs(2f * Frac(w + .5) - 1f); } 
    public static float Wave(double w) { return Wiggle(Frac(w + .25) * 4 - 2) * .5f + .5f; } 
    public static float Pulse(double w) { return 1f - Math.Abs(Wave(w) * 2f - 1f); } 

    // clips out a region around zero, and closes up the gap linearly
    public static double DeadZone(double x, double clip)
    {
        return x < -clip ? x + clip : x > clip ? x - clip : 0.0;
    }

    // ensures that output values are not any smaller than pad, by sliding entire range away from zero by that amount; could just clamp them outward instead
    public static double BoostZone(double x, double pad)
    {
        return x < -double.Epsilon ? x - pad : x > double.Epsilon ? x + pad : 0.0;
    }
    // Attempt to cope with behavior of PID integral resetting when any override slider changes
    // by avoiding changing setting unless it would result in large enough actual difference to matter.
    public static bool SetWithMinDelta(ITerminalProperty<float> p, IMyTerminalBlock b, float v, float tiny = .01f, float mindeltafraction = .07f) 
    { 
        v = MathHelper.Clamp(v, p.GetMinimum(b), p.GetMaximum(b)); 
        if (Math.Abs(v) < tiny) v = 0; // flush tiny #s to zero 
        var old = p.GetValue(b); 
        float mindelta = Math.Abs(old) * mindeltafraction + .002f; 
        if ((v == 0 && old == 0) || Math.Abs(old - v) < mindelta) return false; // close enough already! do not fiddle with it, let gyro spin up 
        p.SetValue(b, v); return true; 
    }
    public static readonly char[] LineSeps = new char[] { '\n','\r','|',';' };
    public static readonly char[] AssignSeps = new char[] { '=', ':' }; 
} 
 
// PRETTY
// prevent visual information overload, ease parsing
public static class Pretty 
{
    static readonly float[] tcache = new float[] { 0f, .1f, .01f, .001f, .0001f };
    public static float NoTiny(float x, int dig = 1)
    {
        return Math.Abs(x) < (dig < tcache.Length ? tcache[dig] : Math.Pow(.1, dig)) ? x*float.Epsilon : (float)Math.Round(x, dig);
    }
    public static string _(float  f) { return NoTiny(f, 1).ToString("g3"); }
    public static string _(double d) { return NoTiny((float)d, 1).ToString("g4"); }

    const string degUnit = " °"; // angular degrees 
    public static string Degrees(double a) { return _((float)a) + degUnit; }
    public static string Radians(double a) { return Degrees(MathHelper.ToDegrees(a)); }
    public static string Degrees(Vector3 a) { return _(a) + degUnit; }
    public static string Radians(Vector3 a) { return Degrees(a * MathHelper.ToDegrees(1)); }
    public static string MultiLine(string name, Vector3 v, string unit) 
    { 
        return     name + "x: " + Pretty._(v.X)
          + '\n' + name + "y: " + Pretty._(v.Y) + ' ' + unit 
          + '\n' + name + "z: " + Pretty._(v.Z); 
    }

    static string oAxSep = " ";
    static readonly char[] iAxSep = new[] { ' ', '\t', ',' };
    public static string _(Vector3 v)
    {
        return _(v.X) + oAxSep + _(v.Y) + oAxSep + _(v.Z);
    }
    public static string _(Vector3D v)
    {
        return _(v.X) + oAxSep + _(v.Y) + oAxSep + _(v.Z);
    }
    public static string _(Quaternion q)
    {
        return _(q.X) + oAxSep + _(q.Y) + oAxSep + _(q.Z) + oAxSep + _(q.W); //q.ToString(); //
    } 
} 
 
// GRAPH 
// Low level graphing support 
// deals with the peculiarities of drawing using Space Engineers text panels 
public static class Graph 
{ 
    public static Module GetModule() 
    { 
        var m = new Module(); 
        m.Name = "Graph"; 
        m.Tick = Update; 
        return m; 
    } 
 
    public const float row2fontsize = 2.1f; // 2 rows
    public static readonly int smChartWidth = Screen.FontColumns(row2fontsize, 8);
 
    public static Color Twirl; 
 
    static void Update() 
    { 
        var t = 1f / 3000 * Time.ams; 
        Twirl = Hue(t); 
        Twirl = Desat(Twirl, .9f - .4f * Utility.Sqr(Utility.Pulse(t/17))); 
        Twirl = Glo(Twirl, .5f + .5f * (float)Math.Pow(Utility.Pulse(t/23) * .2f, 32)); 
    } 
 
    public static Color StatusColor(double status) 
    { 
        status = MathHelper.Saturate(status); 
        var t = 1f / 500 * Time.ams; // 2/s 
        var stat = Hue((1f - status) * .33f); // from green at 0 to red at 1 and beyond // .5f cyan could work 
        stat = Glo(stat, Utility.Pulse(t) * (float)status * .3f); // blink stronger with more error 
        return stat; 
    } 
 
    public static Color Hue(double H) // 0/6:R, 1/6:Y, 2/6:G, 3/6:C, 4/6:B, 5/6:M 
    { // some credit:  T. Nathan Mundhenk 
        var h6 = 6f * Utility.Frac(H); // range reduce angle 
        int i = (int)h6; float f = h6 - i; 
        // boost toward rgb poles only, doesn't look so pastel 
        f *= (i & 1) != 0 ? 2f - f : f; var e = 1f - f; 
        float R, G, B; 
        switch (i % 6) 
        { 
            default:  
            case 0: R = 1; G = f; B = 0; break; 
            case 1: R = e; G = 1; B = 0; break; 
            case 2: R = 0; G = 1; B = f; break; 
            case 3: R = 0; G = e; B = 1; break; 
            case 4: R = f; G = 0; B = 1; break; 
            case 5: R = 1; G = 0; B = e; break; 
        } 
        var c = new Vector3(R, G, B); // math while linear 
        c *= new Vector3(1.1140f-.2990f, 1.1140f-.5870f, 1f) * (1 / c.Length()); // normalize perceptual brightnesses - keep blue, reduce others 
        c *= c; // convert gamma 
        return c; 
    } 
 
    public static Color Glo(Color c, float a) { return Color.Lerp(c, Color.White, a); } 
    public static Color Dim(Color c, float a) { return Color.Lerp(Color.Black, c, a); } 
    public static Color Desat(Color c, float a) { return Color.Lerp(c, new Color(Luma(c)), a); } 
 
    public static float Lumi(Vector3 clinear) { return Vector3.Dot(new Vector3(.2126f, .7152f, .0722f), clinear); } // lumi Y from linear RGB using ITU-R Rec. BT.709 / sRGB 
    public static float Luma(Vector3 cgamma ) { return Vector3.Dot(new Vector3(.2990f, .5870f, .1140f), cgamma ); } // luma Y' from gamma RGB using ITU-R Rec. BT.601-2 luma 
 
    public static string Fraction(double f, int w) // a simple 1-line horizontal bar graph representing a percentage 
    {
        var cfill = (int)(MathHelper.Saturate(f)*w);
        return new string('I', cfill);// + new string(' ', w - cfill); // so the left is the FG (filled) and the right is the BG (empty)
    }

    static int Pos(double v, int width, int center, double maximum)
    {
        return MathHelper.Clamp((int)Math.Round(center*v/maximum) + center, 0, width-1);
    }

    public static string Limit(double v, double limit, double maxi, int w)
    {
        if ((w & 1) == 0) --w; // width should be odd
        if (Math.Abs(v) > maxi) return new string(v < 0 ? '›' : '‹', w); 
        int c = w >> 1; // center
        int lo = Pos(-limit, w, c, maxi);
        var g = new string('∙', lo);
        g = g + new string(' ', w - lo*2) + g;
        int pat = Pos(v, w, c, maxi);
        char nub = v < -limit ? '›' : v > limit ? '‹' : 'I';
        return g.Substring(0, Math.Max(0, pat)) + nub + g.Substring(Math.Min(w, pat + 2));
    } 
}

// VESSEL

// Tracks the state of a vessel.  A vessel is a collection of connected grids. 
// Supports multiple controllers!  (cockpits, control stations, remote controls) 
public static class Vessel 
{    
    public static Module GetModule() 
    { 
        Chart.Title["mass"] = massChart; 
        Chart.Title["grav"] = gravity.gravChart; 
        Chart.Title["pos"] = posChart; 
        Chart.Title["vel"] = velChart; 
        Chart.Title["ori"] = oriChart; 
        Chart.Title["rot"] = rotChart; 
        var m = new Module(); 
        m.Name = "Vessel"; 
        m.Scan = Rescan; 
        m.Tick = Update; 
        m.Parse = Parse; 
        return m; 
    } 
 
    public static IMyShipController Controller = null; // presently active controller, being controlled by a player 
 
    public struct Gravity 
    { 
        public const float StandardG = 9.80665f; // m/s²
        public float Strength;    // present effective acceleration in m/s²
        public Vector3D World;    // scaled by strength (up to 1G) world space at this vessel's location
        public Vector3  WorldDir; // unit length
        public Vector3D Local;    // scaled by strength (up to 1G) ship local space
        public Vector3  LocalDir; // unit length
        public Chart gravChart;

        public void Update(IMyShipController c, Quaternion worldToShip) 
        { 
            if (c == null) { World = WorldDir = Local = LocalDir = new Vector3(); Strength = 0; gravChart.Text = "Misconfigured!"; return; }
            World = c.GetNaturalGravity(); // gravity acceleration vector in m/s² - ships without artificial mass ignore artificial gravity
            Strength = (float)World.Length();
            Local = Vector3.Transform(World, worldToShip);
            LocalDir = Vector3D.Normalize(Local / Strength);
            WorldDir = Vector3D.Normalize(World / Strength);
            if (Vector3D.IsZero(World, .01))
            { // normalized will be bogus, zero them.
                WorldDir = LocalDir = new Vector3();
                //Log.Output("No Gravity!");
                gravChart.Text = "No gravity";
            }
            else 
            { 
                //Log.Output("gravity: " + Pretty._(gravity.Local) + " m/s² (local)"); 
                //Log.Output("gravity: " + Pretty._(gravity.Local*(1/StandardG)) + " G (local)"); 
                //Log.Output("gravity dir: " + Pretty._(gravity.LocalDir) + " (local)"); 
                gravChart.Text = "Gravity: " + Pretty._(Vessel.gravity.Strength) + " G"; 
            }
            gravChart .UpdateColor(0); gravChart.Param = 2.6f; 
        }
    } 
    public static Gravity gravity; // for the active controller, if any; not tracking multiple gravities or altitudes yet. 
 
    public static bool Rescan() 
    { 
        Controllers.Clear(); 
        Scanner.GetBlocksOfType(Controllers); 
        FindController(); 
        if (Controller == null) 
            Log.scan.AppendLine("No controllers found!"); 
        UpdateMass(); 
        UpdateMatrix(); 
        if (gravity.gravChart == null) gravity.gravChart = new Chart();
        return true; 
    } 
 
    // it's pretty crucial that this match the actual ship mass otherwise we may not be able to hover or will overthrust like mad
    public static double Mass; // total in kg
 
    // I'm torn whether to move these into Motion/Rotion or leave here since they're obtained from IMyShipController 
    public static Vector3D Position;

    public static Vector3 LinearVelocity; // in meters/s in world space
    public static Vector3 LinearAcceleration; // in meters/s² in world space

    // an angular velocity vector is a rotation axis scaled by speed in radians/second
    public static Vector3 AngularVelocity; // in radians/s per axis in world space

    public static Quaternion worldToShip; // inverse orientation
    public static Quaternion shipToWorld { get { return Quaternion.Conjugate(worldToShip); } } // from ship controller to world space

    public static Vector3 WorldToShipDir(Vector3 d) { return Vector3.Transform(d, worldToShip); }
    public static Vector3 ShipToWorldDir(Vector3 d) { return Vector3.Transform(d, shipToWorld); }

    static Chart massChart = new Chart(); 
    static Chart posChart = new Chart(); 
    static Chart velChart = new Chart(); 
    static Chart oriChart = new Chart(); 
    static Chart rotChart = new Chart(); 

    public static bool Parse(string[] tokens) 
    { 
        if (tokens.Length == 1) 
        { 
            string varn = tokens[0]; 
            return false; 
        } 
        else 
        { 
            double val; 
            if (!double.TryParse(tokens[1], out val)) return false; 
            string varn = tokens[0]; 
            if (varn == "extramass") {} // ExtraMass = val; 
            else if (varn == "cargomult") {} // CargoMult = val; 
            else return false; 
        } 
        return true; 
    } 
 
    static void FindController() // find the one under control if possible, or use the first controller available otherwise.
    {
        Controller = null;
        foreach (var c in Controllers)
            if (c.IsUnderControl 
             || Controller == null // TODO prefer controller on same grid as PB.
             || (!Controller.IsUnderControl && c.GetValueBool("MainCockpit"))) // prefer main over uncontrolled non-main
                Controller = c;
    }

    static void UpdateMass() 
    { 
        if (Controller == null) { Mass = 0; return; }
        var massinfo = Controller.CalculateShipMass();
        //Mass = massinfo.BaseMass;
        var cargoMass = massinfo.PhysicalMass - massinfo.BaseMass; // - massInfo.PhysicalMass(massinfo.TotalMass - massinfo.BaseMass) / CargoMult; // account for survival inventory multiplier
        //Mass += cargoMass;
        //Mass += ExtraMass; // in case TotalMass doesn't account for something somehow (strangely attached towed ships?  Rocks that aren't in an inventory?)
        Mass = massinfo.PhysicalMass;
        //scanlog.AppendLine("Mass: " + Mass + " kg");
        //scanlog.AppendLine("  Block: " + MassInfo.BaseMass.ToString("F0") + " kg");
        //scanlog.AppendLine("  Cargo: " + cargoMass.ToString("F1") + " kg");
        //scanlog.AppendLine("ExtraMass: " + ExtraMass.ToString("F0") + " kg");
        massChart.Text = "Mass: " + Mass + " kg"; massChart.Param = 2.6f;
        massChart.Text += "\n  Cargo: " + cargoMass.ToString("F1") + " kg";
        //if (CargoMult != 1f) massChart.Text += "\n(cargo x" + CargoMult + ')';
        //if (ExtraMass != 1f) massChart.Text += "\n(extra " + ExtraMass + " kg)";
    }

    static void UpdateMatrix()
    {
        if (Controller != null) // update from shipcontroller
        {
            Position = Controller.WorldMatrix.Translation;
            var Orientation = Quaternion.CreateFromRotationMatrix(Controller.WorldMatrix.GetOrientation());
            worldToShip = Quaternion.Conjugate(Orientation);
            // TODO if it's a remote, determine the configured "forward" direction
            posChart.Text = "Pos: " + Pretty._(Position); posChart.Param = Graph.row2fontsize;
            oriChart.Text = "Ori: " + Pretty._(Orientation); oriChart.Param = Graph.row2fontsize;
        }
    }

    // read gravity, orientation, and position, info about planet if any.  Called each Tick.
    public static void Update()
    {
        if (Controller == null || !Controller.IsUnderControl)
            FindController();
        if (Controller == null) 
        { // can survive without it
            Log.Output("No active controller in group!");
            return;
        }
        UpdateMass();
        UpdateMatrix();
        AnalyzeMotion(); 
        gravity.Update(Controller, worldToShip);
    }

    static void AnalyzeMotion()
    {
        if (Controller == null) { AngularVelocity = LinearVelocity = LinearAcceleration = new Vector3(); return; }
        MyShipVelocities vels = Controller.GetShipVelocities();
        AngularVelocity = vels.AngularVelocity;
        LinearAcceleration = (vels.LinearVelocity - LinearVelocity) / Time.dts;
        LinearVelocity = vels.LinearVelocity;
        velChart.Text = "Vel: " + Pretty._(LinearVelocity); velChart.Param = Graph.row2fontsize;
        rotChart.Text = "Rot: " + Pretty._(AngularVelocity); rotChart.Param = Graph.row2fontsize;
    }

    static List<IMyShipController> Controllers = new List<IMyShipController>(); 
} 

// ROTION
// Manages orientation, rotation, angular velocity, and torque
public static class Rotion
{
    public static Module GetModule() 
    { 
        Chart.Title["steer"] = steerChart; 
        var m = new Module(); 
        m.Name = "Rotion"; 
        m.Scan = Rescan; 
        m.Tick = Update; 
        m.Parse = Parse; 
        m.Save = Save; 
        return m; 
    } 
 
    public const double deg2rad = Math.PI / 180; // see VRageMath.MathHelper.ToRadians(1)  // FIXME move to Utility
    public const double rad2deg = 180 / Math.PI; // see VRageMath.MathHelper.ToDegrees(1)
    public static double RPMLimit = 60; // can reduce to slow ship rotation globally when under script control
    public static double TotalTorque; // of entire vessel, combined, best estimate, in kN

    public static Chart steerChart = new Chart();

    // disable gyro overrides and allow default angular damping to bring vessel to a stop
    public static void NoTurnVessel()
    {
        Explain(Vector3.Zero, "free");
        foreach (var gyro in Gyros)
            TurnGyro(gyro, new Vector3(), false);
    }

    public static float RotCapOnAxis(Vector3 axis) // returns available torque along that axis, in N, assumes axis is unit length 
    { 
        return (float)(TotalTorque * 1000); //(TotalTorque.Dot(axis) * 1000); 
    }

    // takes vessel-local desired angular velocity vector
    public static void RotateVessel(Vector3 lrot) 
    { 
        if (lrot.LengthSquared() > 1e-6f) TurnVessel(lrot, "rot"); 
        else NoTurnVessel();
    }
    // takes vessel-local desired angular velocity vector
    // request vessel to rotate at the stated local angular velocities, ASAP.
    // Gyros respond slowly though.  Changing settings often is a bad idea.
    static DateTime nextSteeringTime = DateTime.Now;
    public static readonly TimeSpan steeringUpdatePeriod = Time.Frequency(10);
    public static void TurnVessel(Vector3 lrot, string why = "turn")
    {
        if (DateTime.Now < nextSteeringTime) return; // STOP FIDDLING with the sliders constantly
        var largest = lrot.Length(); 
        var mini = largest * .15f; // threshold relative to scale of vector as a whole
        if (Math.Abs(lrot.X) < mini) lrot.X = 0; // flush tiny fiddly manipulations to zero
        if (Math.Abs(lrot.Y) < mini) lrot.Y = 0;
        if (Math.Abs(lrot.Z) < mini) lrot.Z = 0;
        Explain(lrot, why);
        //rot *= new Vector3I(0,0,0); if (rot.LengthSquared() < 1e-4f) return; // DEBUG
        var maxr = (float)RPMLimit * MathHelper.RPMToRadiansPerSecond;
        lrot = Vector3.Clamp(lrot, Vector3.MinusOne * maxr, Vector3.One * maxr);
        lrot = Vessel.ShipToWorldDir(lrot);
        foreach (var gyro in Gyros)
            TurnGyro(gyro, lrot, true);
        nextSteeringTime = DateTime.Now + steeringUpdatePeriod;
    }
 
    public static void AngularDampen(Vector3 mask, double v = .0001, double v2 = .04) 
    { 
        if (Vessel.AngularVelocity.LengthSquared() > 2e-5f)
        { // cancel existing velocity, prevent overshoot/oscillation
            var localr = Vessel.WorldToShipDir(Vessel.AngularVelocity) * mask;
            localr *= -(float)(v2*localr.Length() + v); // correct proportionally to error and its square
            if (localr.LengthSquared() > 1e-6) { TurnVessel(localr, "damp"); return; }
        }
        NoTurnVessel();
    }

    //public static void MatchOrientation(Quaternion ori, Vector3 angVel) // TODO 
    //{ // take 3 deltas, and 3 derivative deltas, feed them thru a PID controller, and set the gyros appropriately 
    //    var euler = MyMath.QuaternionToEuler(ori); // VRageMath 
    //}

    static void Explain(Vector3 r, string why) 
    { 
        steerChart.Text = "Steering: " + why + '\n'
         + Pretty.MultiLine("r", r * (float)rad2deg, " °/s") //²? no
        //+ Pretty._(r) + " radian/s"
        //+ Pretty.Radians(r) + "/s"
          ; steerChart.Param = 1.2f; // fit 4 rows
    }

    static string[] Save()
    {
        return new string[] {
            "gyrolimitrpm=" + RPMLimit, 
        };
    }

    static bool Parse(string[] tokens)
    {
        if (tokens.Length == 1) 
        { 
            string varn = tokens[0]; 
            if (varn == "stop" || varn == "reset") NoTurnVessel(); 
            else return false; 
        } else { 
            double val; 
            if (!double.TryParse(tokens[1], out val)) return false; 
            string varn = tokens[0]; 
                 if (varn == "gyrolimitrpm" && val >= 0) RPMLimit = val; 
            else return false; 
        } 
        return true;
    }

    static void Update()
    {
        Log.Output(steerChart.Text);
    }

    static bool Rescan()
    {
        Gyros.Clear();
        Scanner.GetBlocksOfType(Gyros);
        TotalTorque = 0;
        foreach (var g in Gyros)
            InitGyro(g);
        if (Gyros.Count == 0) // need gyros to function normally, but otherwise we can still display orientation.
            Log.scan.AppendLine("No gyros found in group!");
        //foreach (var g in Gyros) Log.scan.AppendLine("'" + g.CustomName + "' at " + (int)(g.GyroPower*100) + '%'
        //    + "rated " + propGyroYaw.GetMaximum(g)*MathHelper.RadiansPerSecondToRPM + " RPM"); 
        return true; // NOT FATAL // Gyros.Count > 0; // should make sure at least some are functioning
    }

    const float SmallGyroTorque = 1.35e5f; // measured by competition with a rotor
    const float LargeGyroTorque = 1.35e7f;

    static void InitGyro(IMyGyro gyro)
    {
        if (propGyroOverride == null)
        {
            propGyroOverride = gyro.GetProperty("Override").AsBool(); // for setting 
            //propGyroPower = gyro.GetProperty("GyroPower").AsFloat();
            propGyroPitch = gyro.GetProperty("Pitch").AsFloat();
            propGyroYaw   = gyro.GetProperty("Yaw"  ).AsFloat();
            propGyroRoll  = gyro.GetProperty("Roll" ).AsFloat();
        }
        //gyro.BlockDefinition.SubtypeId // TODO account for modded gyros FIXME account for off-grid gyros/leverage on main center of mass
        var gtorque = gyro.CubeGrid.GridSize < 1 ? SmallGyroTorque : LargeGyroTorque;
        gtorque *= gyro.GyroPower; if (gyro.GyroPower > 1f) throw new Exception("no, gyropower is a percentage"); // * .01f; //
        TotalTorque += gtorque;
    }

    const float tiny = .1f; 
    const float mindeltafraction = .1f;

    // rot is a world rotation axis scaled by rotation rate in radians/second
    // (an angular velocity vector)
    // Pitch = noseup = ccw x, Yaw = noseleft = ccw y, Roll = ccw z
    static void TurnGyro(IMyGyro g, Vector3 v, bool overrideg)
    {
        propGyroOverride.SetValue(g, overrideg);
        if (overrideg)
        { // must not care what set to when not overridden
            Matrix mworld = g.WorldMatrix.GetOrientation(); // gyro to world
            mworld.TransposeRotationInPlace(); // now world to gyro
            v = Vector3.Transform(v, ref mworld); // now gyro local
            v *= new Vector3I(1, -1, -1); // gyros have backward conventions for angular directions on certain axes, versus how it works out with cross products.
            Utility.SetWithMinDelta(propGyroPitch, g, v.X, tiny, mindeltafraction);
            Utility.SetWithMinDelta(propGyroYaw,   g, v.Y, tiny, mindeltafraction);
            Utility.SetWithMinDelta(propGyroRoll,  g, v.Z, tiny, mindeltafraction);
        }
    }

    static List<IMyGyro> Gyros = new List<IMyGyro>();

    static ITerminalProperty<bool> propGyroOverride; // gyro
    //static ITerminalProperty<float> propGyroPower; // unit: fraction (0..100?)
    static ITerminalProperty<float> propGyroPitch, propGyroYaw, propGyroRoll; // units radian/s, displayed units RPM
}
 
// UPRIGHT
// Keeps Vessel down vector aligned with gravity.
// Uses Rotion module to manage the gyroscope overrides.
public static class Upright 
{ 
    public static Module GetModule() 
    { 
        Chart.Title  ["pitch"  ] = PitchChart; 
        Chart.Title  ["roll"   ] = RollChart; 
        Chart.Title  ["yaw"    ] = TurnYawChart; 
        var m = new Module(); 
        m.Name = "Upright"; 
        m.Tick = Update; 
        m.Parse = Parse; 
        m.Save = Save; 
        return m; 
    } 
    public static double PitchOffsetDegrees = 0; // allows ship to maintain different pitch attitudes
    public static double PitchTiltDegrees = 0; // for use by Hover module to temporarily tilt the ship
    public static double PitchTiltMaxDegrees = 45; // limits +/- PitchTiltDegrees

    public static double PitchAngleClipDegrees = .02; // in degrees - the better pitch is aligned, the less roll will become misaligned
    public static double  RollAngleClipDegrees = .02; // in degrees - match roll precisely if possible, but don't fidget over micro-degrees
    public static double PitchAngleSlopDegrees = .1; // in degrees
    public static double  RollAngleSlopDegrees = .1; // in degrees
    // once it goes past clip + slop limit, it will begin to correct toward the clip boundary 
    public static double AlignAngularResponseP2 = 0; //4; // factor proportional to square of error
    public static double AlignAngularResponseP = 2; // factor converting misalignment (sine) into angular radians/s; overall corrector stiffness; (was GyroSpeedScale)
    public static double AlignAngularResponseI = .1; // integral factor
    // No longer need these since I account for stopping distance
    public static double AlignAngularResponseD = 0; //.1; // forced damping counter to existing velocity, to counter overcorrection and oscillation on massive ships
    public static double AlignAngularResponseD2 = 0; //1.4; // proportional to square of angular velocity, works much better for my purposes than using first derivative
    public static double PitchSpeedScale = 1; // extra factor for pitch, to help deal with ship mass distribution issues
    public static double RollSpeedScale = 1; // extra factor for roll, to help deal with ship mass distribution issues
    public static double ThresholdBoundaryBoost = .001; 
    public static double PitchThresholdCushion = 1; // anisotropic sensitivity
    public static double  RollThresholdCushion = 1; // to threshold violations
    public static double YawSensitivity = 1; // input sensitivity 
    public static double YawRPMLimit = 60; // steering speed limit (small ship gyros will only do 60, large only 30!)
    // TODO try a half time step of the Rotion.steeringUpdatePeriod
    public static TimeSpan OrientPredict = Time.s(0f); //Rotion.steeringUpdatePeriod.TotalSeconds*.5); //.1); // extrapolate velocity into the future to predict and correct constraint violations before they occur; works best with soft response 
    public static double AngularDamping = .5; // empirically tuned to approximate cockpit's angular damping

    static PIDControllerFloat GyroControllerPitch = new PIDControllerFloat();
    static PIDControllerFloat GyroControllerRoll = new PIDControllerFloat();

    public static Chart PitchChart = new Chart();
    public static Chart RollChart = new Chart();
    public static Chart detailChart = new Chart();
    public static Chart TurnYawChart = new Chart(); 

    static void Update() 
    { 
        if (Vessel.Controller == null) return; 
        float steeringYaw = Vessel.Controller.RotationIndicator.Y * (float)YawSensitivity;
        TurnYawChart.Text = "Yaw:\n" + Graph.Limit(steeringYaw, 9, 12  , Graph.smChartWidth); TurnYawChart.Param = Graph.row2fontsize;

        if (Vessel.gravity.Strength < .01) 
        {
            PitchChart.Text = RollChart.Text = "No gravity.";
            PitchChart.Param = RollChart.Param = 2f; 
            return; 
        } 

        var localr = Vessel.WorldToShipDir(Vessel.AngularVelocity); // to local space, radians/s
        var xflocalGravity = Vessel.gravity.LocalDir;
        if (localr.LengthSquared() > float.Epsilon && OrientPredict > new TimeSpan()) // extrapolation
        { // transform local gravity where it will be in the future
            var predictangle = Vessel.AngularVelocity.Length() * OrientPredict.TotalSeconds; // radians
            xflocalGravity = Vector3.Transform(xflocalGravity, Quaternion.CreateFromAxisAngle(Vector3.Normalize(localr), (float)predictangle));
        }
        double a = Rotion.deg2rad * (PitchOffsetDegrees + MathHelper.Clamp(PitchTiltDegrees, -PitchTiltMaxDegrees, PitchTiltMaxDegrees));
        var AlignDir = new Vector3D(0, -Math.Cos(a), Math.Sin(a));
        Vector3 cross = Vector3.Cross(AlignDir, xflocalGravity); // perpendicular to both AlignDir and gravity in ship local space
        //cross *= new Vector3I( 1, 0, 0); // DEBUG isolate axes
        var angles = new Vector3(Math.Asin(cross.X), Math.Asin(cross.Y), Math.Asin(cross.Z)); // convert sines to radian angles
        //Log.Output("lg: " + Pretty(xflocalGravity));
        //Log.Output("Cross: " + Pretty(cross));
        const double border = 2; // ° border
        double limx = Rotion.deg2rad * (PitchAngleClipDegrees+PitchAngleSlopDegrees), lim2x = limx + Rotion.deg2rad*border;
        double limz = Rotion.deg2rad * ( RollAngleClipDegrees+ RollAngleSlopDegrees), lim2z = limz + Rotion.deg2rad*border;
        PitchChart.Text = "Pitch: " + Pretty.Radians(-angles.X) + '\n' + Graph.Limit(-angles.X, limx, lim2x, Graph.smChartWidth);
        RollChart .Text = "Roll:  " + Pretty.Radians(-angles.Z) + '\n' + Graph.Limit(-angles.Z, limz, lim2z, Graph.smChartWidth);
        PitchChart.Param = RollChart.Param = Graph.row2fontsize; // only need 2 lines

        var dampangvel = localr;// * (float)Math.Exp(-Time.dts * AngularDamping); // decayed over time; trying to match normal gyro damping rate
        var yv = dampangvel.Dot(xflocalGravity); // the old motion around the gravity vector
        // only part of rotation not around gravity vector should count as override
        // preconditioners for dead zones
        var clippedlimits = angles;
        clippedlimits.X = (float)Utility.DeadZone(clippedlimits.X, PitchAngleClipDegrees * Rotion.deg2rad);
        clippedlimits.Z = (float)Utility.DeadZone(clippedlimits.Z,  RollAngleClipDegrees * Rotion.deg2rad);
        bool pastlimit = Math.Abs(clippedlimits.X) > PitchAngleSlopDegrees * Rotion.deg2rad || Math.Abs(clippedlimits.Z) > RollAngleSlopDegrees * Rotion.deg2rad;
        var response = -clippedlimits;
        response.X = (float)Utility.BoostZone(response.X, ThresholdBoundaryBoost);
        response.Z = (float)Utility.BoostZone(response.Z, ThresholdBoundaryBoost);
        // since the Targets were never set, they're still at zero always. 
        GyroControllerPitch.Kp2 = GyroControllerRoll.Kp2 = (float)AlignAngularResponseP2;
        GyroControllerPitch.Kp = GyroControllerRoll.Kp = (float)AlignAngularResponseP;
        GyroControllerPitch.Ki = GyroControllerRoll.Ki = (float)AlignAngularResponseI;
        GyroControllerPitch.Kd = GyroControllerRoll.Kd = (float)AlignAngularResponseD;
        GyroControllerPitch.Kd2= GyroControllerRoll.Kd2= (float)AlignAngularResponseD2;
        response.X = GyroControllerPitch.Advance(Time.dts, response.X) * (float)PitchSpeedScale;
        response.Z = GyroControllerRoll .Advance(Time.dts, response.Z) * (float) RollSpeedScale;
        response.X = Utility.ClampAbs(response.X, PIDControllerFloat.MaxVelocityForDistanceStoprate(Math.Abs(clippedlimits.X), Rotion.RotCapOnAxis(Vector3.Right   ) / (float)Vessel.Mass)); 
        response.Z = Utility.ClampAbs(response.Z, PIDControllerFloat.MaxVelocityForDistanceStoprate(Math.Abs(clippedlimits.Z), Rotion.RotCapOnAxis(Vector3.Backward) / (float)Vessel.Mass));
        Log.Output("pitcherr: " + Pretty._(GyroControllerPitch.Error));
        Log.Output("pitchint: " + Pretty._(GyroControllerPitch.Integral));
        //if (Math.Abs(response.X) < 5e-8f) response.X = 0f; // don't let imprecision in corrected axis cause misalignment burst just because the other axis isn't aligned!
        //if (Math.Abs(response.Z) < 5e-8f) response.Z = 0f; // but the lower level stuff in Rotion will handle that later
        PitchChart.TextColor = Graph.StatusColor(Math.Abs(response.X));
         RollChart.TextColor = Graph.StatusColor(Math.Abs(response.Z));
        var overridev = response; //Vector3.Reject(response, xflocalGravity); // measure full response including damping, but ignoring old yaw
        var overridepower2 = overridev.LengthSquared(); // cutoff measurement
        // pretend rotation around gravity was "un-steered" and decelerating / damping normally
        // instead of jerking to a screeching halt when other correction happens
        // effectively transfers yv motion into new motion vector
        yv *= -(float)(AlignAngularResponseD + Math.Abs(yv) * AlignAngularResponseD2); // angular damping - does this need to account for time somehow?  Bear in mind gyros take radians/s and add to existing angular momentum
        response += xflocalGravity * yv; // mix in the damping motion around gravity vector
        var uactive = Vessel.Controller != null && Vessel.Controller.IsUnderControl;
        if (uactive) response += xflocalGravity * (float)Math.Min(steeringYaw, YawRPMLimit * MathHelper.RPMToRadiansPerSecond); // mix in user steering around gravity vector (NOT ship Y)
        bool steering; // when user is yawing, trying to control the vehicle
        steering = uactive && Math.Abs(steeringYaw) > 1e-2f; //Math.Abs(yv) > 2e-2f;
        //Log.Output("response: " + Pretty.Radians(response) + "/s");
        bool overridegyros = true; //pastlimit; // now that I can get user input, just leave overridden always
        if (!overridegyros) Rotion.NoTurnVessel();
        else Rotion.TurnVessel(response, "align"); // given ship controller local coords
    }

    static bool Parse(string[] tokens) 
    { 
        string varn = tokens[0];
        if (tokens.Length == 1)
        {
            if (varn == "stop" || varn == "reset") 
                Rotion.NoTurnVessel(); 
            else return false;
        }
        else
        {
            double val;
            if (!double.TryParse(tokens[1], out val)) return false;
            if (varn == "pitchoffsetdegrees" || varn == "pitchofs") // legacy name - TODO REMOVE
                PitchOffsetDegrees = MathHelper.ToDegrees(MathHelper.WrapAngle(MathHelper.ToRadians((float)val)));
            else if (varn == "orientpredictseconds" && val >= 0)
                OrientPredict = Time.s(val);
            else if (varn == "tiltmaxdegrees" && val >= 0)
                PitchTiltMaxDegrees = (float)val;
            else if ((varn == "pitchangleclipdegrees" || varn == "pitchclip") && val >= 0)
                PitchAngleClipDegrees = val;
            else if ((varn == "rollangleclipdegrees" || varn == "rollclip") && val >= 0)
                RollAngleClipDegrees = val;
            else if ((varn == "pitchangleslopdegrees" || varn == "pitchslop") && val >= 0)
                PitchAngleSlopDegrees = val;
            else if ((varn == "rollangleslopdegrees" || varn == "rollslop") && val >= 0)
                RollAngleSlopDegrees = val;
            else if (varn == "steeringsensitivityyaw" && val >= 0)
                YawSensitivity = val;
            else if (varn == "steeringyawlimitrpm" && val >= 0)
                YawRPMLimit = val;
            else if (varn == "alignangularresponsep2")
                AlignAngularResponseP2 = val;
            else if (varn == "alignangularresponsep")
                AlignAngularResponseP = val;
            else if (varn == "alignangularresponsei")
                AlignAngularResponseI = val;
            else if (varn == "alignangularresponsed")
                AlignAngularResponseD = val;
            else if (varn == "alignangularresponsed2")
                AlignAngularResponseD2 = val;
//            else if (varn == "alignangulardamping" && val >= 0)
//                AngularDamping = val;
             else return false;
        }
        return true;
    }

    static string[] Save() 
    { 
        return new string[] {
            "pitchoffsetdegrees=" + PitchOffsetDegrees, 
            "tiltmaxdegrees=" + PitchTiltMaxDegrees,
            "orientpredictseconds=" + OrientPredict.TotalSeconds, 
            "pitchangleclipdegrees=" + PitchAngleClipDegrees,
            "rollangleclipdegrees=" + RollAngleClipDegrees,
            "pitchangleslopdegrees=" + PitchAngleSlopDegrees,
            "rollangleslopdegrees=" + RollAngleSlopDegrees,
            "steeringsensitivityyaw=" + YawSensitivity,
            "steeringyawlimitrpm=" + YawRPMLimit,
            "alignangularresponsep2=" + AlignAngularResponseP2,
            "alignangularresponsep=" + AlignAngularResponseP,
            "alignangularresponsei=" + AlignAngularResponseI,
            "alignangularresponsed=" + AlignAngularResponseD,
            "alignangularresponsed2=" + AlignAngularResponseD2,
//            "alignangulardamping=" + AngularDamping,
        }; 
    }
} 

// LOG 
 
public static class Log 
{ 
    public static Module GetModule() 
    { 
        var m = new Module(); 
        m.Name = "Log"; 
        m.Tick = Update; 
        return m; 
    }

    public static Action<string> Echo = s => {}; // patch from ctor, so all modules can easily access it 
     
    public static void Output(string s) { log.AppendLine(s); } 
    public static void Format(string format, params object[] args) { log.AppendFormat(format, args); } 
      
    public static System.Text.StringBuilder log = new System.Text.StringBuilder(); // gets scanLog appended then cleared after each Show
    public static System.Text.StringBuilder scan = new System.Text.StringBuilder(); // avoid rebuilding each Run, cleared each Scan
    public static System.Text.StringBuilder diagnostic = new System.Text.StringBuilder(); // cleared each Scan, written to screen CustomData
    
    static void Update()
    {
        log.Append(scan);
        Chart.Standard.Text = log.ToString();
        Chart.Standard.Texture = false;
        log.Clear();
    }
} 

// CHART 
// can show on a text panel visually 
public class Chart 
{ 
    static Chart() 
    { 
        Title["logo"] = Logo; 
        //Subtype["Wide"] = Logo; 
        Subtype["Corner"] = Logo; 
    } 
 
    public static Module GetModule() 
    { 
        var m = new Module(); 
        m.Name = "Chart"; 
        m.Tick = Update; 
        return m; 
    } 
    public string Text = null; // text or texture name (will show "OFFLINE" if null either way) 
    public float Param = 0f; // override font size or image time if > 0
    public bool Texture = false; // if true, Text is texture name to show  
    public bool Compact = true; // if true, font size is doubled on wide panels, halved on large ship corner panels 
 
    public Color TextColor = Color.White; 
    public Color BackColor = Color.Black; 
 
    public void UpdateColor(double err) 
    { 
        TextColor = err <= 0 ? Graph.Twirl : Graph.StatusColor(err); 
    } 
 
    public void MakeFill(string quant, double fill) 
    { 
        Text = quant + ": " + fill.ToString("P1") //(int)Math.Round(fill * 100) + " %" // (fill * 100).ToString("F0") + "%"; // 
            + '\n' + Graph.Fraction(fill, Graph.smChartWidth); 
        Param = Graph.row2fontsize; 
        TextColor = Graph.StatusColor(1f - fill); 
    } 
 
    static void Update() 
    { 
        if (Standard.Text == null) Standard.Text = Log.scan.ToString(); // + Log.diagnostic; 
        Logo.TextColor = new Color(Utility.Wave(1f / 24000 * Time.ams));
        Logo.BackColor = Graph.Glo(Graph.Hue(1f / 12000 * Time.ams), .8f*(float)Math.Pow(Utility.Pulse(Utility.Wave(1f / 7000 * Time.ams)), 16));
        Logo.Param = Utility.Tri(1f / 70000 * Time.ams) * .4f + 1.9f; // slow pingpong fontsize between 1.9 and 2.3
    } 
 
    public void Show(IMyTextPanel display) 
    { 
        if (Text == null || Texture) 
            display.ShowTextureOnScreen(); 
        if (Text != null) 
            if (Texture) 
            { 
                foreach (var image in Text.Split('|')) 
                    display.AddImageToSelection(image, true); 
            } 
            else 
            { 
                Screen.SetBackgroundColor(display, BackColor); 
                Screen.SetTextColor(display, TextColor); 
                if (Compact && Screen.IsWide(display)) Param *= 2; 
                else if (Screen.IsCorner(display) && (!Compact || display.CubeGrid.GridSize > 1f)) Param *= .5f; 
                if (Param > 0) Screen.SetFontSize(display, Param); 
                display.ClearImagesFromSelection(); 
                display.WritePublicText(Text); 
                display.ShowPublicTextOnScreen(); 
                display.CustomData = Log.diagnostic.ToString(); // using CustomData as private (diagnostic) output for LCDs 
            } 
    } 
 
    public void Show(IMyLightingBlock light) 
    { 
        var c = BackColor != Color.Black ? BackColor : TextColor; 
        Screen.SetLightColor(light, c); 
        // CustomData is selector for lights (input) so do not write to it 
    } 
 
    public static Chart Logo = new Chart();
    public static Chart Standard = new Chart(); // default
    
    public static Dictionary<string, Chart> Title   = new Dictionary<string, Chart>(); // map each Public Title for screens (CustomData for other blocks) substring to its graph; only put lowercase strings as keys! 
    public static Dictionary<string, Chart> Subtype = new Dictionary<string, Chart>(); // map each Subtype to its graph (case sensitive) 
 
    static string GetSubtype(IMyTerminalBlock block) { return block.BlockDefinition.SubtypeId; } // case sensitive 
    static string GetName(IMyTerminalBlock block) { return block.CustomName.ToLowerInvariant(); } // case insensitive 
    static string GetCustomData(IMyTerminalBlock block) { return block.CustomData.ToLowerInvariant(); } // case insensitive 
    static string GetTitle(IMyTextPanel screen) { return screen.GetPublicTitle().ToLowerInvariant(); } // case insensitive 
    static Chart Select(IMyTextPanel screen, Dictionary<string, Chart> dict, Func<IMyTextPanel, string> info) 
    { 
        foreach (var p in dict) if (info(screen).Contains(p.Key)) return p.Value; 
        return null; 
    } 
    static Chart Select(IMyLightingBlock light, Dictionary<string, Chart> dict, Func<IMyLightingBlock, string> info) 
    { 
        foreach (var p in dict) if (info(light).Contains(p.Key)) return p.Value; 
        return null; 
    } 
     
    public static Chart ForScreen(IMyTextPanel screen) 
    { // select by title or LCD type 
        Chart chart = null; 
        chart = chart ?? Select(screen, Title,   GetTitle); 
        chart = chart ?? Select(screen, Subtype, GetSubtype); 
        chart = chart ?? Standard; 
        return chart; 
    } 
    public static Chart ForLight(IMyLightingBlock light) 
    { // select by CustomData or subtype     
        Chart chart = null; 
        chart = chart ?? Select(light, Title,  GetCustomData); 
        chart = chart ?? Select(light, Subtype, GetSubtype);  
        return chart; 
    } 
    public static void ShowFor(IMyTextPanel screen) 
    { 
        Chart chart; 
        chart = ForScreen(screen); 
        if (chart != null) chart.Show(screen); 
    } 
    public static void ShowFor(IMyLightingBlock light) 
    { 
        var chart = ForLight(light); 
        if (chart != null) chart.Show(light); 
    } 
} 

// SCREEN

public static class Screen 
{ 
    public static Module GetModule() 
    { 
        var m = new Module(); 
        m.Name = "Screen"; 
        m.Scan = Rescan; 
        m.Tick = Update; 
        m.Parse = Parse; 
        return m; 
    } 
 
    static void Update() 
    { 
        foreach (var s in Screens) Chart.ShowFor(s); 
        foreach (var l in  Lights) Chart.ShowFor(l); 
    } 
 
    static bool Rescan() 
    { 
        Screens.Clear(); Lights.Clear(); 
        Scanner.GetBlocksOfType(Screens); 
        Scanner.GetBlocksOfType(Lights); 
        foreach (var t in Screens) InitScreen(t); 
        foreach (var l in  Lights) InitLight (l); 
        if (Screens.Count == 0) Log.scan.AppendLine("No displays in group!  Minimal diagnostics."); // not fatal
        return true;
    } 
    
    static bool Parse(string[] tokens)
    {
        string varn = tokens[0];
        if (tokens.Length == 1)
        {
            if (varn == "stop" || varn == "reset") Off(); // overload to turn "off" displays/lights
            else return false;
        }
        else
        {
            double dvalue;
            if (!double.TryParse(tokens[1], out dvalue)) return false;
        }
        return true;
    } 
 
    public static int PipColumns(double pipsw, int charPips)
    {
        return (int)Math.Floor(pipsw / charPips);
    }
    public static int PipRows(double pipsh)
    { 
        return (int)Math.Floor(pipsh / 33);
    }
    public static int FontColumns(double fontSize, int charPips)
    { // how many columns of char can fit on a standard screen at given fontSize?
        return PipColumns(584 / fontSize, charPips);
    }
    // aids scaling Charts shown on the wrong kind of panel
    public static bool IsWide(IMyTextPanel screen)
    {
        return screen.BlockDefinition.SubtypeId.Contains("Wide");
    }
    public static bool IsCorner(IMyTextPanel screen)
    {
        return screen.BlockDefinition.SubtypeId.Contains("Corner");
    }

    public static void SetBackgroundColor(IMyTextPanel lcd, Color c) 
    { 
        c.A = 255; // NO ALPHA 
        textBackgroundColor.SetValue(lcd, c); 
    } 
 
    public static void SetTextColor(IMyTextPanel lcd, Color c) 
    { 
        textFontColor.SetValue(lcd, c); 
    }

    public static void SetFontSize(IMyTextPanel lcd, float size) 
    { 
        textFontSize.SetValue(lcd, size); 
    }
    public static float GetFontSize(IMyTextPanel lcd) 
    { 
        return textFontSize.GetValue(lcd); 
    }

    // turns all LCDs off to show "OFFLINE" or texture.
    public static void Off() 
    { 
        foreach (var s in Screens)
        { // clear screens on save, so that on Load we know anything written was from the loading script
            s.WritePublicText("");
            s.CustomData = "";
            if (textBackgroundColor != null)
                textBackgroundColor.SetValue(s, Color.Black);
            s.AddImageToSelection("Offline", true);
            s.ShowTextureOnScreen();
        }
        foreach (var l in Lights)
        { 
            SetLightColor(l, Color.Black); 
        }
    }

    public static void SetLightColor(IMyLightingBlock l, Color c) 
    { 
        lightColor.SetValue(l, c); 
    }

    static void InitScreen(IMyTextPanel s)
    { 
        if (textBackgroundColor == null)
            textBackgroundColor = s.GetProperty("BackgroundColor").AsColor();
        if (textFontColor == null)
            textFontColor = s.GetProperty("FontColor").AsColor();
        if (textFontSize == null)
            textFontSize = s.GetProperty("FontSize").AsFloat();
    }

    static void InitLight(IMyLightingBlock l)
    { 
        if (lightColor == null)
            lightColor = l.GetProperty("Color").AsColor();
    }

    static List<IMyTextPanel> Screens = new List<IMyTextPanel>(); 
    static List<IMyLightingBlock> Lights = new List<IMyLightingBlock>(); 
 
    static ITerminalProperty<float> textFontSize;
    static ITerminalProperty<Color> textFontColor;
    static ITerminalProperty<Color> textBackgroundColor; 
    static ITerminalProperty<Color> lightColor;
} 
 
// GRAVITYALIGNER 
 
static class GravityAligner 
{ 
    public static Module GetModule() 
    { 
        var m = new Module(); 
        m.Name = "GravityAligner"; 
        return m; 
    } 
    public static void DisplayUsage() 
    { 
        Log.Echo(ScriptTitle + @""); 
    } 
    public static void StopOverrides() 
    { 
        Rotion.NoTurnVessel(); 
    } 
}

public void Save()
{
    Module.Store();
} 
 
public Program()
{
    Chart.Logo.Text = ScriptTitle;
    Chart.Standard = Upright.detailChart;
    // setup program execution structure by modules
    Module.All.AddArray(new Module[] { 
        Time.GetModule(), 
        Graph.GetModule(), 
        Vessel.GetModule(), 
        Rotion.GetModule(), 
        Upright.GetModule(), 
        GravityAligner.GetModule(), 
        Log.GetModule(), 
        Chart.GetModule(), 
        Screen.GetModule(), 
    });
    Scanner.GroupName = GroupName;
    Scanner.Update();
}

public void Main(string arg)
{
  try {
    if (!Module.Commands(arg, true)) GravityAligner.DisplayUsage();
  } catch (Exception e) { 
    GravityAligner.StopOverrides(); 
    throw new Exception("Main:\n" + e.Message); 
  } 
}

    }
}
