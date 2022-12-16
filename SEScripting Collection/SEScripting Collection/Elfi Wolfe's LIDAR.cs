using System;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRageMath;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;
using System.Text;

class LIDAR : MyGridProgram
{
#region in-game
    //------------------------------------------------------------
    // ADN - Easy Lidar Homing Script v3.0
    //------------------------------------------------------------

    //----- Refer To Steam Workshop Discussion Section For Variables Definition -----
    //
    //  [This sets the default missile launch type. Overridable by single trigger Run Action with Arguments MODE:<missileLaunchType> before starting Timer Loop]
    //
    //  0 = Lidar Homing Mode using launching ship's R_LIDAR for initial lock-on and R_FORWARD for aiming.
    //  1 = Lidar Homing Mode using coordinates from launching ship's R_TARGET for initial lock-on.
    //  2 = Lidar Homing Mode using launching ship's R_LIDAR for initial lock-on and R_FORWARD for aiming with initial Camera Guidance.
    //  3 = Camera Guided Mode using launching ship's R_FORWARD for direction tracking. R_FORWARD can also be a Turret.
    //  4 = Cruise Mode using launching ship's R_TARGET as GPS coordinates.
    //  5 = Offset Lidar Homing Mode using launching ship's R_LIDAR for initial lock-on and R_FORWARD for aiming.
    //  6 = Fixed Glide Mode using launching ship's R_FORWARD for one time aiming, then continues to glide on that fixed direction.
    //  7 = Lidar Homing Mode solely using launching ship's R_LIDAR for lock-on and R_FORWARD for aiming (Semi-Active Style Guidance).
    //  8 = Turret AI Homing Mode using missile's mounted turret for guidance.
    //  99 = Launch missile in Dummy Mode. This is used for testing if the missile detaches properly at the correct connection and display missile detected configurations.
    //
    int missileLaunchType = 0;

//Type of block to disconnect missile from launching ship: 0 = Merge Block, 1 = Rotor, 2 = Connector, 3 = Merge Block And Any Locked Connectors, 4 = Rotor And Any Locked Connectors, 99 = No detach required
int missileDetachPortType = 0;

//Spin Missile By This RPM After Launch Clearance
int spinAmount = 0;

//Whether to perform a vertical takeoff for the launching procedure
bool verticalTakeoff = false;

//Whether to fly straight first until LOCK command is given via R_TARGET Text Panel to start homing. Activating this will ignore launchSeconds variable
bool waitForHomingTrigger = false;

//Whether to allow missile to read Custom Data of the R_TARGET to get command during missile homing
bool enableMissileCommand = true;

//------------------------------ Inter Grid Communications Configuration ------------------------------

string missileId = null;
string missileGroup = null;
string allowedSenderId = null;

//------------------------------ Reference Block Name Configuration ------------------------------

string strShipRefLidar = "R_LIDAR";
string strShipRefForward = "R_FORWARD";
string strShipRefTargetPanel = "R_TARGET";

string strShipRefNotMissileTag = "NOT_MISSILE";

//This line configures missile activation commands to allow missile to perform action during launch
string missileActivationCommands = "";

//This line configures missile trigger commands to allow missile to perform action based on proximity and duration
string missileTriggerCommands = "";

//By default all gyroscopes, thrusters and merge blocks will be considered for use. Setting a value here limits the script to use specific set of blocks
string strGyroscopesTag = "";
string strThrustersTag = "";
string strDetachPortTag = "";
string strDirectionRefBlockTag = "";

//For missile lock alert
string strLockTriggerBlockTag = "R_ALERT";
string strLockTriggerAction = "PlaySound";

//For debugging purposes
string strStatusDisplayPrefix = "<D>";

//------------------------------ Missile Handling Configuration ------------------------------

double driftVectorReduction = 1.2;
double launchSeconds = 1;

//By default, script will determine whether to use these parameters. Setting a value forces script to use it
bool? boolDrift = null;
bool? boolLeadTarget = null;
bool? boolNaturalDampener = null;

//------------------------------ Lidar Lock On Configuration ------------------------------

//Minimum and maximum lock-on distance
float LIDAR_MIN_LOCK_DISTANCE = 50;
float LIDAR_MAX_LOCK_DISTANCE = 3000;

//Number of ticks before next Lidar lock-on attempt is triggered
int LIDAR_REFRESH_INTERVAL = 5;

//Whether to exclude targeting friendly targets
bool excludeFriendly = false;

//------------------------------ Above Is User Configuration Section. This Section Is For PID Tuning ------------------------------

double DEF_SMALL_GRID_P = 300;
double DEF_SMALL_GRID_I = 0.1;
double DEF_SMALL_GRID_D = 100;

double DEF_BIG_GRID_P = 50;
double DEF_BIG_GRID_I = 0.5;
double DEF_BIG_GRID_D = 4;

bool useDefaultPIDValues = true;

double AIM_P = 0;
double AIM_I = 0;
double AIM_D = 0;
double AIM_LIMIT = 60;

double INTEGRAL_WINDUP_LIMIT = 0;

//------------------------------ Script Parameters Configuration ------------------------------

int MERGE_SEPARATE_WAIT_THRESHOLD = 60;

double TURRET_AI_PN_CONSTANT = 5;
int TURRET_AI_AVERAGE_SIZE = 1;

bool outputMissileStatus = true;

//------------------------------ Below Is Main Script Body ------------------------------

List<IMyCameraBlock> shipRefLidars = null;
IMyTerminalBlock shipRefForward = null;
IMyTextPanel shipRefTargetPanel = null;

List<IMyCameraBlock> missileLidars = null;
IMyShipController remoteControl = null;
IMyTerminalBlock refForwardBlock = null;
IMyTerminalBlock refDownwardBlock = null;

int lidarStaggerIndex = 0;

IMyLargeTurretBase homingTurret = null;
VectorAverageFilter turretVectorFilter = null;

IMyTerminalBlock alertBlock = null;
IMyTerminalBlock statusDisplay = null;

IMyTerminalBlock notMissile = null;
double notMissileRadius = 0;

bool homingReleaseLock = false;

bool commsPositionSet = false;
Vector3D commsPosition = default(Vector3D);

bool commsForwardSet = false;
RayD commsForward = default(RayD);

bool commsLidarTargetSet = false;
MyDetectedEntityInfo commsLidarTarget = default(MyDetectedEntityInfo);

List<IMyTerminalBlock> gyroscopes = null;
string[] gyroYawField = null;
string[] gyroPitchField = null;
string[] gyroRollField = null;
float[] gyroYawFactor = null;
float[] gyroPitchFactor = null;
float[] gyroRollFactor = null;

List<IMyTerminalBlock> thrusters = null;
float[] thrustValues = null;

List<IMyTerminalBlock> launchThrusters = null;

MatrixD refWorldMatrix = default(MatrixD);
MatrixD refLookAtMatrix = default(MatrixD);
bool refForwardReverse = false;

bool forwardIsTurret = false;

Vector3D refForwardPosition = new Vector3D();
Vector3D refForwardVector = new Vector3D();
bool refForwardSet = false;

Vector3D naturalGravity = new Vector3D();
double naturalGravityLength = 0;

Vector3D midPoint = new Vector3D();
Vector3D driftVector = new Vector3D();
double speed = 0;
double rpm = 0;

Vector3D lastMidPoint = new Vector3D();
Vector3D lastNormal = new Vector3D();

MyDetectedEntityInfo lidarTargetInfo = default(MyDetectedEntityInfo);
int lastLidarTriggerClock = Int32.MinValue;

Vector3D targetPosition = new Vector3D();
Vector3D lastTargetPosition = new Vector3D();
Vector3D offsetTargetPosition = new Vector3D();

bool targetPositionSet = false;
int lastTargetPositionClock = 0;

Vector3D targetVector = new Vector3D();
double distToTarget = 0;

Vector3D targetDirection = new Vector3D();
double targetSpeed = 0;
double targetRadius = 0;

double targetYawAngle = 0;
double targetPitchAngle = 0;
double targetRollAngle = 0;

double lastYawError = 0;
double lastYawIntegral = 0;
double lastPitchError = 0;
double lastPitchIntegral = 0;
double lastRollError = 0;
double lastRollIntegral = 0;

int subCounter = 0;
int subMode = 0;
int mode = -2;
int clock = 0;
bool init = false;

IMyTerminalBlock detachBlock = null;
int detachBlockType = -1;    //-1 = Not Found, 0 = Merge Blocks, 1 = Rotors, 2 = Connectors

List<KeyValuePair<double, string[]>> rpmTriggerList = null;
List<KeyValuePair<double, string[]>> distTriggerList = null;
List<KeyValuePair<int, string[]>> timeTriggerList = null;
bool haveTriggerCommands = false;

Random rnd = new Random();

static float coneLimit = 0;
static double sideScale = 0;

const double RPM_FACTOR = 1800 / Math.PI;
const double ACOS_FACTOR = 180 / Math.PI;
const float GYRO_FACTOR = (float)(Math.PI / 30);
const double RADIAN_FACTOR = Math.PI / 180;

Vector3D Y_VECTOR = new Vector3D(0, -1, 0);
Vector3D Z_VECTOR = new Vector3D(0, 0, -1);
Vector3D POINT_ZERO = new Vector3D(0, 0, 0);

void Main(string arguments)
{
//---------- Initialization And General Controls ----------

if (!init)
{
if (subMode == 0)  //Check for configuration command
{
subMode = 1;

missileId = Me.GetId().ToString();
missileGroup = null;

if (Me.CustomData.Length > 0)
{
ProcessCustomConfiguration();
}

if (arguments != null && arguments.Length > 0)
{
ProcessConfigurationCommand(arguments);
return;
}
}

if (subMode == 1)  //Missile still on launching ship's grid
{
InitLaunchingShipRefBlocks();

if (shipRefForward != null)
{
refForwardPosition = shipRefForward.WorldMatrix.Translation;
refForwardVector = (forwardIsTurret ? CalculateTurretViewVector(shipRefForward as IMyLargeTurretBase) : shipRefForward.WorldMatrix.Forward);
}

if (!DetachFromGrid())
{
throw new Exception("--- Initialization Failed ---");
}

subCounter = 0;
subMode = (missileDetachPortType == 99 ? 3 : 2);
return;
}
else if (subMode == 2)  //Missile waiting for successful detachment from launching ship
{
bool isDetached = false;

if (notMissile != null)
{
isDetached = (notMissile.CubeGrid != Me.CubeGrid);
}
else if (detachBlockType == 0)
{
isDetached = !((detachBlock as IMyShipMergeBlock).IsConnected);
}
else if (detachBlockType == 1)
{
isDetached = !((detachBlock as IMyMechanicalConnectionBlock).IsAttached);
}
else if (detachBlockType == 2)
{
isDetached = ((detachBlock as IMyShipConnector).Status != MyShipConnectorStatus.Connected);
}

if (isDetached)
{
subMode = 3;
return;
}
else
{
subCounter++;

if (subCounter >= MERGE_SEPARATE_WAIT_THRESHOLD)
{
Echo("Error: Missile detach failed.");
throw new Exception("--- Initialization Failed ---");
}

return;
}
}
else if (subMode == 3)  //Missile successfully detached and currently initializing
{
if (missileDetachPortType == 3 || missileDetachPortType == 4)
{
DetachLockedConnectors();
}

if (notMissile != null)
{
notMissileRadius = ComputeBlockGridDiagonalVector(notMissile).Length() / 2.0;
}

if (!InitMissileBlocks())
{
throw new Exception("--- Initialization Failed ---");
}
}

if (missileLaunchType == 99)
{
subMode = 0;
mode = 99;
clock = 0;
}
else
{
if (waitForHomingTrigger)
{
subCounter = int.MaxValue;
}
else
{
if (missileLaunchType == 6)
{
refForwardSet = true;
}

subCounter = (int)(launchSeconds * 60);
}

FireThrusters(verticalTakeoff ? launchThrusters : thrusters, true);

subMode = 0;
mode = -1;
clock = 0;
}

if (missileActivationCommands != null && missileActivationCommands.Length > 0)
{
ExecuteTriggerCommand(missileActivationCommands);
}

init = true;
return;
}

//---------- Modes And Controls ----------

clock += 1;

CalculateParameters();

if (arguments != null && arguments.Length > 0)
{
ProcessCommunicationMessage(arguments);
return;
}

if (enableMissileCommand)
{
ProcessMissileCommand(shipRefTargetPanel.CustomData);
}

if (mode == -1)  //Launching
{
if (waitForHomingTrigger)
{
if (homingReleaseLock)
{
subCounter = 0;
}
}

if (subCounter > 0)
{
subCounter--;
}
else
{
if (verticalTakeoff)
{
FireThrusters(launchThrusters, false);
FireThrusters(thrusters, true);
}

SetGyroOverride(true);

if (spinAmount > 0)
{
SetGyroRoll(spinAmount);
}

distToTarget = 1000000;

if (missileLaunchType == 3 || missileLaunchType == 4 || missileLaunchType == 6)
{
if (missileTriggerCommands != null && missileTriggerCommands.Length > 0)
{
ExecuteTriggerCommand(missileTriggerCommands);
}
}

lastTargetPosition = targetPosition = GetFlyStraightVector();

subCounter = 0;
subMode = 0;
mode = missileLaunchType;
}
}
else if (mode == 0)  //Lidar Homing With Initial Shipborne Lidar Lock-On
{
if (subMode == 0)  //Initial Lock-On
{
if ((lastLidarTriggerClock + LIDAR_REFRESH_INTERVAL <= clock) || commsLidarTargetSet)
{
lastLidarTriggerClock = clock;

bool targetFound = false;

if (commsLidarTargetSet)
{
commsLidarTargetSet = false;

if (IsValidLidarTarget(ref commsLidarTarget, ref refWorldMatrix))
{
distToTarget = Vector3D.Distance(commsLidarTarget.Position, refWorldMatrix.Translation);
targetSpeed = commsLidarTarget.Velocity.Length();
targetDirection = (targetSpeed > 0 ? new Vector3D(commsLidarTarget.Velocity) / targetSpeed : new Vector3D());
targetRadius = Vector3D.Distance(commsLidarTarget.BoundingBox.Min, commsLidarTarget.BoundingBox.Max);

lidarTargetInfo = commsLidarTarget;
lastTargetPositionClock = clock;

targetPositionSet = targetFound = true;
}
}
else if (shipRefForward != null)
{
MatrixD shipRefWorldMatrix = shipRefForward.WorldMatrix;
Vector3D shipRefForwardVector = (forwardIsTurret ? CalculateTurretViewVector(shipRefForward as IMyLargeTurretBase) : shipRefWorldMatrix.Forward);
Vector3D shipRefTargetPosition = shipRefWorldMatrix.Translation + (shipRefForwardVector * LIDAR_MAX_LOCK_DISTANCE);

IMyCameraBlock aimLidar = GetAvailableLidar(shipRefLidars, shipRefTargetPosition, 0, lidarStaggerIndex++);
if (aimLidar != null)
{
MyDetectedEntityInfo entityInfo = aimLidar.Raycast(shipRefTargetPosition);
if (!entityInfo.IsEmpty())
{
if (IsValidLidarTarget(ref entityInfo, ref shipRefWorldMatrix))
{
distToTarget = Vector3D.Distance(entityInfo.Position, refWorldMatrix.Translation);
targetSpeed = entityInfo.Velocity.Length();
targetDirection = (targetSpeed > 0 ? new Vector3D(entityInfo.Velocity) / targetSpeed : new Vector3D());
targetRadius = Vector3D.Distance(entityInfo.BoundingBox.Min, entityInfo.BoundingBox.Max);

lidarTargetInfo = entityInfo;
lastTargetPositionClock = clock;

targetPositionSet = targetFound = true;
}
}
}
}

if (targetFound)
{
double overshootDistance = targetRadius / 2;

IMyCameraBlock syncLidar = GetAvailableLidar(missileLidars, lidarTargetInfo.Position, overshootDistance, lidarStaggerIndex++);
if (syncLidar != null)
{
Vector3D testTargetPosition = lidarTargetInfo.Position + (Vector3D.Normalize(lidarTargetInfo.Position - syncLidar.GetPosition()) * overshootDistance);

MyDetectedEntityInfo entityInfo = syncLidar.Raycast(testTargetPosition);
if (!entityInfo.IsEmpty())
{
if (entityInfo.EntityId == lidarTargetInfo.EntityId)
{
TriggerLockAlert();

if (missileTriggerCommands != null && missileTriggerCommands.Length > 0)
{
ExecuteTriggerCommand(missileTriggerCommands);
}

subCounter = 0;
subMode = 1;
}
}
}
}
}

if (targetPositionSet)
{
targetPosition = lidarTargetInfo.Position + (lidarTargetInfo.Velocity / 60 * (clock - lastTargetPositionClock));

if (boolLeadTarget == true)
{
CalculateLeadParameters();
}
}
}
else if (subMode == 1)  //Lidar Homing
{
PerformLidarLogic();

if (boolLeadTarget == true)
{
CalculateLeadParameters();
}
}

CalculateTargetParameters();
AimAtTarget();

if (boolNaturalDampener == true)
{
AimAtNaturalGravity();
}

if (haveTriggerCommands)
{
ProcessTriggerCommands();
}
}
else if (mode == 1)  //Lidar Homing With Initial Shipborne GPS Coordinates Lock-On
{
if (subMode == 0)  //Initial Lock-On
{
if (commsPositionSet)
{
commsPositionSet = false;

targetPosition = commsPosition;
targetPositionSet = true;
}
else
{
Vector3D parsedVector;
if (shipRefTargetPanel != null && ParseCoordinates(shipRefTargetPanel.GetPublicTitle(), out parsedVector))
{
targetPosition = parsedVector;
targetPositionSet = true;
}
else
{
lastTargetPosition = targetPosition = GetFlyStraightVector();
targetPositionSet = false;
}
}

if (targetPositionSet && (GetMissileMidPoint() - targetPosition).Length() < 1)
{
lastTargetPosition = targetPosition = GetFlyStraightVector();
targetPositionSet = false;
}

if (targetPositionSet)
{
if (lastLidarTriggerClock + LIDAR_REFRESH_INTERVAL <= clock)
{
lastLidarTriggerClock = clock;

double overshootDistance = targetRadius / 2;

IMyCameraBlock syncLidar = GetAvailableLidar(missileLidars, targetPosition, overshootDistance, lidarStaggerIndex++);
if (syncLidar != null)
{
Vector3D testTargetPosition = targetPosition + (Vector3D.Normalize(targetPosition - syncLidar.GetPosition()) * overshootDistance);

MyDetectedEntityInfo entityInfo = syncLidar.Raycast(testTargetPosition);
if (!entityInfo.IsEmpty())
{
if (IsValidLidarTarget(ref entityInfo, ref refWorldMatrix))
{
distToTarget = Vector3D.Distance(entityInfo.Position, refWorldMatrix.Translation);
targetSpeed = entityInfo.Velocity.Length();
targetDirection = (targetSpeed > 0 ? new Vector3D(entityInfo.Velocity) / targetSpeed : new Vector3D());
targetRadius = Vector3D.Distance(entityInfo.BoundingBox.Min, entityInfo.BoundingBox.Max);

lidarTargetInfo = entityInfo;
lastTargetPositionClock = clock;

TriggerLockAlert();

if (missileTriggerCommands != null && missileTriggerCommands.Length > 0)
{
ExecuteTriggerCommand(missileTriggerCommands);
}

subCounter = 0;
subMode = 1;
}
}
}
}
}

if (boolLeadTarget == true)
{
CalculateTargetInfo();
CalculateLeadParameters();
}
}
else if (subMode == 1)  //Lidar Homing
{
PerformLidarLogic();

if (boolLeadTarget == true)
{
CalculateLeadParameters();
}
}

CalculateTargetParameters();
AimAtTarget();

if (boolNaturalDampener == true)
{
AimAtNaturalGravity();
}

if (haveTriggerCommands)
{
ProcessTriggerCommands();
}
}
else if (mode == 2)  //Lidar Homing With Initial Shipborne Camera Guidance
{
if (subMode == 0)  //Initial Camera Guidance
{
if (commsForwardSet)
{
commsForwardSet = false;

refForwardPosition = commsForward.Position;
refForwardVector = commsForward.Direction;
}
else if (shipRefForward != null)
{
refForwardPosition = shipRefForward.WorldMatrix.Translation;
refForwardVector = (forwardIsTurret ? CalculateTurretViewVector(shipRefForward as IMyLargeTurretBase) : shipRefForward.WorldMatrix.Forward);
}

Vector3D shipToMissileVector = midPoint - refForwardPosition;
Vector3D missileToViewLineVector = Vector3D.Reject(shipToMissileVector, refForwardVector);

double extraDistanceExtend = Math.Min(Math.Max(5.6713 * missileToViewLineVector.Length(), speed * 2), speed * 4);
extraDistanceExtend += (shipToMissileVector - missileToViewLineVector).Length();

targetPosition = refForwardPosition + (refForwardVector * extraDistanceExtend);
targetPositionSet = true;

if (lastLidarTriggerClock + LIDAR_REFRESH_INTERVAL <= clock)
{
lastLidarTriggerClock = clock;

Vector3D shipRefTargetPosition = refForwardPosition + (refForwardVector * LIDAR_MAX_LOCK_DISTANCE);

IMyCameraBlock syncLidar = GetAvailableLidar(missileLidars, shipRefTargetPosition, 0, lidarStaggerIndex++);
if (syncLidar != null)
{
MyDetectedEntityInfo entityInfo = syncLidar.Raycast(shipRefTargetPosition);
if (!entityInfo.IsEmpty())
{
if (IsValidLidarTarget(ref entityInfo, ref refWorldMatrix))
{
distToTarget = Vector3D.Distance(entityInfo.Position, refWorldMatrix.Translation);
targetSpeed = entityInfo.Velocity.Length();
targetDirection = (targetSpeed > 0 ? new Vector3D(entityInfo.Velocity) / targetSpeed : new Vector3D());
targetRadius = Vector3D.Distance(entityInfo.BoundingBox.Min, entityInfo.BoundingBox.Max);

lidarTargetInfo = entityInfo;
lastTargetPositionClock = clock;

TriggerLockAlert();

if (missileTriggerCommands != null && missileTriggerCommands.Length > 0)
{
ExecuteTriggerCommand(missileTriggerCommands);
}

subCounter = 0;
subMode = 1;
}
}
}
}
}
else if (subMode == 1)  //Lidar Homing
{
PerformLidarLogic();

if (boolLeadTarget == true)
{
CalculateLeadParameters();
}
}

CalculateTargetParameters();
AimAtTarget();

if (boolNaturalDampener == true)
{
AimAtNaturalGravity();
}

if (haveTriggerCommands)
{
ProcessTriggerCommands();
}
}
else if (mode == 3)  //Camera Guided Mode
{
if (commsForwardSet)
{
commsForwardSet = false;

refForwardPosition = commsForward.Position;
refForwardVector = commsForward.Direction;
}
else if (shipRefForward != null)
{
refForwardPosition = shipRefForward.WorldMatrix.Translation;
refForwardVector = (forwardIsTurret ? CalculateTurretViewVector(shipRefForward as IMyLargeTurretBase) : shipRefForward.WorldMatrix.Forward);
}

Vector3D shipToMissileVector = midPoint - refForwardPosition;
Vector3D missileToViewLineVector = Vector3D.Reject(shipToMissileVector, refForwardVector);

double extraDistanceExtend = Math.Min(Math.Max(5.6713 * missileToViewLineVector.Length(), speed * 2), speed * 4);
extraDistanceExtend += (shipToMissileVector - missileToViewLineVector).Length();

targetPosition = refForwardPosition + (refForwardVector * extraDistanceExtend);
targetPositionSet = true;

CalculateTargetParameters();
AimAtTarget();

if (boolNaturalDampener == true)
{
AimAtNaturalGravity();
}

if (haveTriggerCommands)
{
ProcessTriggerCommands();
}
}
else if (mode == 4)  //Cruise Mode
{
if (commsPositionSet)
{
commsPositionSet = false;

targetPosition = commsPosition;
targetPositionSet = true;
}
else if (shipRefTargetPanel != null)
{
Vector3D parsedVector;
if (ParseCoordinates(shipRefTargetPanel.GetPublicTitle(), out parsedVector))
{
targetPosition = parsedVector;
targetPositionSet = true;
}
}

if (boolLeadTarget == true)
{
CalculateTargetInfo();
CalculateLeadParameters();
}

CalculateTargetParameters();
AimAtTarget();

if (boolNaturalDampener == true)
{
AimAtNaturalGravity();
}

if (haveTriggerCommands)
{
ProcessTriggerCommands();
}
}
else if (mode == 5)  //Offset Lidar Homing With Initial Shipborne Lidar Lock-On
{
if (subMode == 0)  //Initial Lock-On
{
if ((lastLidarTriggerClock + LIDAR_REFRESH_INTERVAL <= clock) || commsLidarTargetSet)
{
lastLidarTriggerClock = clock;

bool targetFound = false;

if (commsLidarTargetSet)
{
commsLidarTargetSet = false;

if (IsValidLidarTarget(ref commsLidarTarget, ref refWorldMatrix))
{
distToTarget = Vector3D.Distance(commsLidarTarget.Position, refWorldMatrix.Translation);
targetSpeed = commsLidarTarget.Velocity.Length();
targetDirection = (targetSpeed > 0 ? new Vector3D(commsLidarTarget.Velocity) / targetSpeed : new Vector3D());
targetRadius = Vector3D.Distance(commsLidarTarget.BoundingBox.Min, commsLidarTarget.BoundingBox.Max);

if (commsLidarTarget.HitPosition != null)
{
offsetTargetPosition = Vector3D.Transform(commsLidarTarget.HitPosition.Value - commsLidarTarget.Position, commsLidarTarget.Orientation);
}
else
{
offsetTargetPosition = POINT_ZERO;
}

lidarTargetInfo = commsLidarTarget;
lastTargetPositionClock = clock;

targetPositionSet = targetFound = true;
}
}
else if (shipRefForward != null)
{
MatrixD shipRefWorldMatrix = shipRefForward.WorldMatrix;
Vector3D shipRefForwardVector = (forwardIsTurret ? CalculateTurretViewVector(shipRefForward as IMyLargeTurretBase) : shipRefWorldMatrix.Forward);
Vector3D shipRefTargetPosition = shipRefWorldMatrix.Translation + (shipRefForwardVector * LIDAR_MAX_LOCK_DISTANCE);

IMyCameraBlock aimLidar = GetAvailableLidar(shipRefLidars, shipRefTargetPosition, 0, lidarStaggerIndex++);
if (aimLidar != null)
{
MyDetectedEntityInfo entityInfo = aimLidar.Raycast(shipRefTargetPosition);
if (!entityInfo.IsEmpty())
{
if (IsValidLidarTarget(ref entityInfo, ref shipRefWorldMatrix))
{
distToTarget = Vector3D.Distance(entityInfo.Position, refWorldMatrix.Translation);
targetSpeed = entityInfo.Velocity.Length();
targetDirection = (targetSpeed > 0 ? new Vector3D(entityInfo.Velocity) / targetSpeed : new Vector3D());
targetRadius = Vector3D.Distance(entityInfo.BoundingBox.Min, entityInfo.BoundingBox.Max);

if (entityInfo.HitPosition != null)
{
offsetTargetPosition = Vector3D.Transform(entityInfo.HitPosition.Value - entityInfo.Position, entityInfo.Orientation);
}
else
{
offsetTargetPosition = POINT_ZERO;
}

lidarTargetInfo = entityInfo;
lastTargetPositionClock = clock;

targetPositionSet = targetFound = true;
}
}
}
}

if (targetFound)
{
double overshootDistance = targetRadius / 2;

IMyCameraBlock syncLidar = GetAvailableLidar(missileLidars, lidarTargetInfo.Position, overshootDistance, lidarStaggerIndex++);
if (syncLidar != null)
{
Vector3D testTargetPosition = lidarTargetInfo.Position + (Vector3D.Normalize(lidarTargetInfo.Position - syncLidar.GetPosition()) * overshootDistance);

MyDetectedEntityInfo entityInfo = syncLidar.Raycast(testTargetPosition);
if (!entityInfo.IsEmpty())
{
if (entityInfo.EntityId == lidarTargetInfo.EntityId)
{
TriggerLockAlert();

if (missileTriggerCommands != null && missileTriggerCommands.Length > 0)
{
ExecuteTriggerCommand(missileTriggerCommands);
}

subCounter = 0;
subMode = 1;
}
}
}
}
}

if (targetPositionSet)
{
targetPosition = lidarTargetInfo.Position + (lidarTargetInfo.Velocity / 60 * (clock - lastTargetPositionClock));
targetPosition += Vector3D.Transform(offsetTargetPosition, MatrixD.Invert(lidarTargetInfo.Orientation));

if (boolLeadTarget == true)
{
CalculateLeadParameters();
}
}
}
else if (subMode == 1)  //Lidar Homing
{
targetPosition = lidarTargetInfo.Position + (lidarTargetInfo.Velocity / 60 * (clock - lastTargetPositionClock));
targetPosition += Vector3D.Transform(offsetTargetPosition, MatrixD.Invert(lidarTargetInfo.Orientation));

if (lastLidarTriggerClock + LIDAR_REFRESH_INTERVAL <= clock)
{
lastLidarTriggerClock = clock;

bool targetFound = false;
double overshootDistance = targetRadius / 2;

IMyCameraBlock aimLidar = GetAvailableLidar(missileLidars, targetPosition, overshootDistance, lidarStaggerIndex++);
if (aimLidar != null)
{
Vector3D testTargetPosition = targetPosition + (Vector3D.Normalize(targetPosition - aimLidar.GetPosition()) * overshootDistance);

MyDetectedEntityInfo entityInfo = aimLidar.Raycast(testTargetPosition);
if (!entityInfo.IsEmpty())
{
if (entityInfo.EntityId == lidarTargetInfo.EntityId)
{
distToTarget = Vector3D.Distance(entityInfo.Position, refWorldMatrix.Translation);
targetSpeed = entityInfo.Velocity.Length();
targetRadius = Vector3D.Distance(entityInfo.BoundingBox.Min, entityInfo.BoundingBox.Max);

lidarTargetInfo = entityInfo;
lastTargetPositionClock = clock;

targetPosition = entityInfo.Position;
targetFound = true;
}
}
}

targetPositionSet = targetFound;
}

if (boolLeadTarget == true)
{
CalculateLeadParameters();
}
}

CalculateTargetParameters();
AimAtTarget();

if (boolNaturalDampener == true)
{
AimAtNaturalGravity();
}

if (haveTriggerCommands)
{
ProcessTriggerCommands();
}
}
else if (mode == 6)  //Fixed Glide Mode
{
if (!refForwardSet)
{
if (commsForwardSet)
{
commsForwardSet = false;

refForwardPosition = commsForward.Position;
refForwardVector = commsForward.Direction;
refForwardSet = true;
}
else if (shipRefForward != null)
{
refForwardPosition = shipRefForward.WorldMatrix.Translation;
refForwardVector = (forwardIsTurret ? CalculateTurretViewVector(shipRefForward as IMyLargeTurretBase) : shipRefForward.WorldMatrix.Forward);
refForwardSet = true;
}
}

Vector3D shipToMissileVector = midPoint - refForwardPosition;
Vector3D missileToViewLineVector = Vector3D.Reject(shipToMissileVector, refForwardVector);

double extraDistanceExtend = Math.Min(Math.Max(5.6713 * missileToViewLineVector.Length(), speed * 2), speed * 4);
extraDistanceExtend += (shipToMissileVector - missileToViewLineVector).Length();

targetPosition = refForwardPosition + (refForwardVector * extraDistanceExtend);
targetPositionSet = true;

CalculateTargetParameters();
AimAtTarget();

if (boolNaturalDampener == true)
{
AimAtNaturalGravity();
}

if (haveTriggerCommands)
{
ProcessTriggerCommands();
}
}
else if (mode == 7)  //Lidar Homing With Shipborne Lidar Lock-On (Semi-Active Style Guidance)
{
if (subMode == 0)  //Initial Lock-On
{
if ((lastLidarTriggerClock + LIDAR_REFRESH_INTERVAL <= clock) || commsLidarTargetSet)
{
lastLidarTriggerClock = clock;

if (commsLidarTargetSet)
{
commsLidarTargetSet = false;

if (IsValidLidarTarget(ref commsLidarTarget, ref refWorldMatrix))
{
distToTarget = Vector3D.Distance(commsLidarTarget.Position, refWorldMatrix.Translation);
targetSpeed = commsLidarTarget.Velocity.Length();
targetDirection = (targetSpeed > 0 ? new Vector3D(commsLidarTarget.Velocity) / targetSpeed : new Vector3D());
targetRadius = Vector3D.Distance(commsLidarTarget.BoundingBox.Min, commsLidarTarget.BoundingBox.Max);

lidarTargetInfo = commsLidarTarget;
lastTargetPositionClock = clock;

targetPositionSet = true;

TriggerLockAlert();

if (missileTriggerCommands != null && missileTriggerCommands.Length > 0)
{
ExecuteTriggerCommand(missileTriggerCommands);
}

subCounter = 0;
subMode = 1;
}
}
else if (shipRefForward != null)
{
MatrixD shipRefWorldMatrix = shipRefForward.WorldMatrix;
Vector3D shipRefForwardVector = (forwardIsTurret ? CalculateTurretViewVector(shipRefForward as IMyLargeTurretBase) : shipRefWorldMatrix.Forward);
Vector3D shipRefTargetPosition = shipRefWorldMatrix.Translation + (shipRefForwardVector * LIDAR_MAX_LOCK_DISTANCE);

IMyCameraBlock aimLidar = GetAvailableLidar(shipRefLidars, shipRefTargetPosition, 0, lidarStaggerIndex++);
if (aimLidar != null)
{
MyDetectedEntityInfo entityInfo = aimLidar.Raycast(shipRefTargetPosition);
if (!entityInfo.IsEmpty())
{
if (IsValidLidarTarget(ref entityInfo, ref shipRefWorldMatrix))
{
distToTarget = Vector3D.Distance(entityInfo.Position, refWorldMatrix.Translation);
targetSpeed = entityInfo.Velocity.Length();
targetDirection = (targetSpeed > 0 ? new Vector3D(entityInfo.Velocity) / targetSpeed : new Vector3D());
targetRadius = Vector3D.Distance(entityInfo.BoundingBox.Min, entityInfo.BoundingBox.Max);

lidarTargetInfo = entityInfo;
lastTargetPositionClock = clock;

targetPositionSet = true;

TriggerLockAlert();

if (missileTriggerCommands != null && missileTriggerCommands.Length > 0)
{
ExecuteTriggerCommand(missileTriggerCommands);
}

subCounter = 0;
subMode = 1;
}
}
}
}
}

if (targetPositionSet)
{
targetPosition = lidarTargetInfo.Position + (lidarTargetInfo.Velocity / 60 * (clock - lastTargetPositionClock));

if (boolLeadTarget == true)
{
CalculateLeadParameters();
}
}
}
else if (subMode == 1)  //Lidar Homing
{
targetPosition = lidarTargetInfo.Position + (lidarTargetInfo.Velocity / 60 * (clock - lastTargetPositionClock));

if ((lastLidarTriggerClock + LIDAR_REFRESH_INTERVAL <= clock) || commsLidarTargetSet)
{
lastLidarTriggerClock = clock;

bool targetFound = false;
double overshootDistance = targetRadius / 2;

if (commsLidarTargetSet)
{
commsLidarTargetSet = false;

if (commsLidarTarget.EntityId == lidarTargetInfo.EntityId)
{
distToTarget = Vector3D.Distance(commsLidarTarget.Position, refWorldMatrix.Translation);
targetSpeed = commsLidarTarget.Velocity.Length();
targetDirection = (targetSpeed > 0 ? new Vector3D(commsLidarTarget.Velocity) / targetSpeed : new Vector3D());
targetRadius = Vector3D.Distance(commsLidarTarget.BoundingBox.Min, commsLidarTarget.BoundingBox.Max);

lidarTargetInfo = commsLidarTarget;
lastTargetPositionClock = clock;

targetPosition = commsLidarTarget.Position;
targetFound = true;
}
}
else if (shipRefForward != null)
{
IMyCameraBlock aimLidar = GetAvailableLidar(shipRefLidars, targetPosition, overshootDistance, lidarStaggerIndex++);
if (aimLidar != null)
{
Vector3D testTargetPosition = targetPosition + (Vector3D.Normalize(targetPosition - aimLidar.GetPosition()) * overshootDistance);

MyDetectedEntityInfo entityInfo = aimLidar.Raycast(testTargetPosition);
if (!entityInfo.IsEmpty())
{
if (entityInfo.EntityId == lidarTargetInfo.EntityId)
{
distToTarget = Vector3D.Distance(entityInfo.Position, refWorldMatrix.Translation);
targetSpeed = entityInfo.Velocity.Length();
targetDirection = (targetSpeed > 0 ? new Vector3D(entityInfo.Velocity) / targetSpeed : new Vector3D());
targetRadius = Vector3D.Distance(entityInfo.BoundingBox.Min, entityInfo.BoundingBox.Max);

lidarTargetInfo = entityInfo;
lastTargetPositionClock = clock;

targetPosition = entityInfo.Position;
targetFound = true;
}
}
}
}

targetPositionSet = targetFound;
}

if (boolLeadTarget == true)
{
CalculateLeadParameters();
}
}

CalculateTargetParameters();
AimAtTarget();

if (boolNaturalDampener == true)
{
AimAtNaturalGravity();
}

if (haveTriggerCommands)
{
ProcessTriggerCommands();
}
}
else if (mode == 8)  //Turret AI Homing
{
if (subMode == 0)  //Initialization
{
if (homingTurret != null)
{
turretVectorFilter = new VectorAverageFilter(TURRET_AI_AVERAGE_SIZE);

homingTurret.EnableIdleRotation = false;
homingTurret.ApplyAction("OnOff_On");

subCounter = 0;
subMode = 1;
}
}
else if (subMode == 1)  //Seeking For Target
{
if (homingTurret.HasTarget)
{
lastTargetPosition = refForwardVector = CalculateTurretViewVector(homingTurret);
turretVectorFilter.Set(ref refForwardVector);

if (missileTriggerCommands != null && missileTriggerCommands.Length > 0)
{
ExecuteTriggerCommand(missileTriggerCommands);
}

subCounter = 0;
subMode = 2;
}
}
else if (subMode == 2)  //Turret Target Locked
{
if (homingTurret.HasTarget)
{
refForwardVector = CalculateTurretViewVector(homingTurret);
turretVectorFilter.Filter(ref refForwardVector, out refForwardVector);

targetVector = refForwardVector;

if (boolLeadTarget == true)
{
targetVector += (refForwardVector - lastTargetPosition) * TURRET_AI_PN_CONSTANT;
targetVector.Normalize();

lastTargetPosition = refForwardVector;
}
}
else
{
targetVector = refForwardVector;
}

if (boolDrift == true && speed >= 5)
{
targetVector = (targetVector * speed) - (driftVector / driftVectorReduction * 0.5);
targetVector.Normalize();
}

targetVector = Vector3D.TransformNormal(targetVector, refLookAtMatrix);
targetVector.Normalize();

if (Double.IsNaN(targetVector.Sum))
{
targetVector = new Vector3D(Z_VECTOR);
}

AimAtTarget();

if (haveTriggerCommands)
{
ProcessTriggerCommands();
}
}

if (boolNaturalDampener == true)
{
AimAtNaturalGravity();
}
}

if (statusDisplay != null)
{
if (mode == -2)
{
DisplayStatus("Idle");
}
else if (mode == -1)
{
DisplayStatus("Launching");
}
else if (mode == 0 || mode == 1 || mode == 5 || mode == 7)
{
if (subMode == 0)
{
DisplayStatus("Initial Lock");
}
else if (subMode == 1)
{
DisplayStatus((targetPositionSet ? "Lock" : "Trace") + ": [" + Math.Round(targetPosition.GetDim(0), 2) + "," + Math.Round(targetPosition.GetDim(1), 2) + "," + Math.Round(targetPosition.GetDim(2), 2) + "]");
}
else
{
DisplayStatus("-");
}
}
else if (mode == 2)
{
if (subMode == 0)
{
DisplayStatus("Initial Camera Lock");
}
else if (subMode == 1)
{
DisplayStatus((targetPositionSet ? "Lock" : "Trace") + ": [" + Math.Round(targetPosition.GetDim(0), 2) + "," + Math.Round(targetPosition.GetDim(1), 2) + "," + Math.Round(targetPosition.GetDim(2), 2) + "]");
}
else
{
DisplayStatus("-");
}
}
else if (mode == 3)
{
DisplayStatus("Camera");
}
else if (mode == 4)
{
DisplayStatus("Cruise: [" + Math.Round(targetPosition.GetDim(0), 2) + "," + Math.Round(targetPosition.GetDim(1), 2) + "," + Math.Round(targetPosition.GetDim(2), 2) + "]");
}
else if (mode == 6)
{
DisplayStatus("Fixed Glide");
}
else if (mode == 8)
{
if (subMode == 2)
{
DisplayStatus("Turret Locked");
}
else if (subMode == 0 || subMode == 1)
{
DisplayStatus("Initial Lock");
}
else
{
DisplayStatus("-");
}
}
else
{
DisplayStatus("-");
}
}

if (outputMissileStatus)
{
string statusCode;
switch (mode)
{
case -2:
statusCode = "-";
break;
case -1:
statusCode = (waitForHomingTrigger ? "W" : (subCounter > 0 ? "F" : "K"));
break;
case 0:
case 1:
case 5:
case 7:
statusCode = (subMode == 0 ? "K" : (targetPositionSet ? "L" : "T"));
break;
case 2:
statusCode = (subMode == 0 ? "C" : (targetPositionSet ? "L" : "T"));
break;
case 3:
statusCode = "C";
break;
case 4:
statusCode = "D";
break;
case 6:
statusCode = "G";
break;
case 8:
statusCode = (subMode == 2 ? "U" : "K");
break;
default:
statusCode = "-";
break;
}
Echo("ST:" + mode + ":" + subMode + ":" + (waitForHomingTrigger ? 0 : subCounter) + ":" + clock + ":" + statusCode + ":" +
Math.Round(targetPosition.GetDim(0), 5) + ":" + Math.Round(targetPosition.GetDim(1), 5) + ":" + Math.Round(targetPosition.GetDim(2), 5) + ":" +
Math.Round(targetRadius, 5) + ":");
}
}

//------------------------------ Miscellaneous Methods ------------------------------

void DisplayStatus(string statusMsg)
{
if (statusDisplay != null)
{
statusDisplay.CustomName = strStatusDisplayPrefix + " Mode: " + mode + ", " + statusMsg;
}
}

void PerformLidarLogic()
{
targetPosition = lidarTargetInfo.Position + (lidarTargetInfo.Velocity / 60 * (clock - lastTargetPositionClock));

if (lastLidarTriggerClock + LIDAR_REFRESH_INTERVAL <= clock)
{
lastLidarTriggerClock = clock;

bool targetFound = false;
double overshootDistance = targetRadius / 2;

IMyCameraBlock aimLidar = GetAvailableLidar(missileLidars, targetPosition, overshootDistance, lidarStaggerIndex++);
if (aimLidar != null)
{
Vector3D testTargetPosition = targetPosition + (Vector3D.Normalize(targetPosition - aimLidar.GetPosition()) * overshootDistance);

MyDetectedEntityInfo entityInfo = aimLidar.Raycast(testTargetPosition);
if (!entityInfo.IsEmpty())
{
if (entityInfo.EntityId == lidarTargetInfo.EntityId)
{
distToTarget = Vector3D.Distance(entityInfo.Position, refWorldMatrix.Translation);
targetSpeed = entityInfo.Velocity.Length();
targetDirection = (targetSpeed > 0 ? new Vector3D(entityInfo.Velocity) / targetSpeed : new Vector3D());
targetRadius = Vector3D.Distance(entityInfo.BoundingBox.Min, entityInfo.BoundingBox.Max);

lidarTargetInfo = entityInfo;
lastTargetPositionClock = clock;

targetPosition = entityInfo.Position;
targetFound = true;
}
}
}

targetPositionSet = targetFound;
}
}

void TriggerLockAlert()
{
if (alertBlock != null)
{
if (alertBlock.HasAction(strLockTriggerAction))
{
alertBlock.ApplyAction(strLockTriggerAction);
}
}
}

Vector3D GetMissileMidPoint()
{
return (Me.CubeGrid.GridIntegerToWorld(Me.CubeGrid.Min) + Me.CubeGrid.GridIntegerToWorld(Me.CubeGrid.Max)) / 2;
}

Vector3D GetFlyStraightVector()
{
return (driftVector * 1000000) + midPoint;
}

//------------------------------ Missile And Target Information Methods ------------------------------

void CalculateParameters()
{
//---------- Calculate Missile Related Variables ----------

refWorldMatrix = refForwardBlock.WorldMatrix;
refLookAtMatrix = MatrixD.CreateLookAt(POINT_ZERO, (refForwardReverse ? refWorldMatrix.Backward : refWorldMatrix.Forward), refWorldMatrix.Up);

if (remoteControl != null)
{
midPoint = remoteControl.GetPosition();
driftVector = remoteControl.GetShipVelocities().LinearVelocity;
speed = driftVector.Length();

naturalGravity = remoteControl.GetNaturalGravity();
naturalGravityLength = naturalGravity.Length();
naturalGravity = (naturalGravityLength > 0 ? naturalGravity / naturalGravityLength : POINT_ZERO);
}
else
{
midPoint = GetMissileMidPoint();
driftVector = midPoint - lastMidPoint;
speed = driftVector.Length();

naturalGravity = driftVector;
naturalGravityLength = naturalGravity.Length();
naturalGravity = (naturalGravityLength > 0 ? naturalGravity / naturalGravityLength : POINT_ZERO);

lastMidPoint = midPoint;
}

rpm = Math.Acos(lastNormal.Dot(refWorldMatrix.Up)) * RPM_FACTOR;
lastNormal = refWorldMatrix.Up;
}

void CalculateTargetInfo()
{
if (targetPositionSet)
{
targetDirection = targetPosition - lastTargetPosition;
targetSpeed = targetDirection.Length();

if (targetSpeed > 0)
{
targetDirection = targetDirection / targetSpeed;
targetSpeed = targetSpeed * 60 / (clock - lastTargetPositionClock);
}

lastTargetPosition = targetPosition;
lastTargetPositionClock = clock;

targetPositionSet = false;
}
else
{
targetPosition = lastTargetPosition + ((targetDirection * targetSpeed) / 60 * (clock - lastTargetPositionClock));
}
}

void CalculateLeadParameters()
{
if (targetSpeed > 0)
{
Vector3D aimPosition = ComputeIntersectionPoint(targetDirection, targetPosition, targetSpeed, midPoint, speed);
if (!Double.IsNaN(aimPosition.Sum))
{
targetPosition = aimPosition;
}
}
}

void CalculateTargetParameters()
{
//---------- Calculate Target Parameters ----------

targetVector = targetPosition - midPoint;
distToTarget = targetVector.Length();
targetVector = targetVector / distToTarget;

if (boolDrift == true && speed >= 5)
{
targetVector = (targetVector * speed) - (driftVector / driftVectorReduction);
targetVector.Normalize();
}

targetVector = Vector3D.TransformNormal(targetVector, refLookAtMatrix);
targetVector.Normalize();

if (Double.IsNaN(targetVector.Sum))
{
targetVector = new Vector3D(Z_VECTOR);
}
}

Vector3D CalculateTurretViewVector(IMyLargeTurretBase turret)
{
Vector3D direction;
Vector3D.CreateFromAzimuthAndElevation(turret.Azimuth, turret.Elevation, out direction);

return Vector3D.TransformNormal(direction, turret.WorldMatrix);
}

//------------------------------ Missile Lock-On And Leading Methods ------------------------------

Vector3D ComputeIntersectionPoint(Vector3D targetDirection, Vector3D targetLocation, double targetSpeed, Vector3D currentLocation, double currentSpeed)
{
//---------- Calculate Impact Point ----------

//targetDirection Must Be Normalized
double a = (targetSpeed * targetSpeed) - (currentSpeed * currentSpeed);
double b = (2 * targetDirection.Dot(targetLocation - currentLocation) * targetSpeed);
double c = (targetLocation - currentLocation).LengthSquared();

double t;

if (a == 0)
{
t = -c / a;
}
else
{
//Use Formula To Find Root: t = ( -b +- sqrt(b^2 - 4ac) ) / 2a
double u = (b * b) - (4 * a * c);
if (u <= 0)
{
//Root Cannot Be Found. Target Unreachable
return new Vector3D(Double.NaN, Double.NaN, Double.NaN);
}
u = Math.Sqrt(u);

double t1 = (-b + u) / (2 * a);
double t2 = (-b - u) / (2 * a);

t = (t1 > 0 ? (t2 > 0 ? (t1 < t2 ? t1 : t2) : t1) : t2);
}

if (t < 0)
{
return new Vector3D(Double.NaN, Double.NaN, Double.NaN);
}
else
{
return targetLocation + (targetDirection * targetSpeed * t);
}
}

IMyCameraBlock GetAvailableLidar(List<IMyCameraBlock> lidars, Vector3D aimPoint, double overshootDistance, int indexOffset)
{
if (lidars.Count > 0)
{
for (int i = 0; i < lidars.Count; i++)
{
IMyCameraBlock lidar = lidars[(i + indexOffset) % lidars.Count];

MatrixD lidarWorldMatrix = lidar.WorldMatrix;
Vector3D aimVector = aimPoint - lidarWorldMatrix.Translation;
double distance = aimVector.Length();

if (lidar.CanScan(distance + overshootDistance))
{
Vector3D scaleLeft = sideScale * lidarWorldMatrix.Left;
Vector3D scaleUp = sideScale * lidarWorldMatrix.Up;

if (sideScale >= 0)
{
if (aimVector.Dot(lidarWorldMatrix.Forward + scaleLeft) >= 0 &&
aimVector.Dot(lidarWorldMatrix.Forward - scaleLeft) >= 0 &&
aimVector.Dot(lidarWorldMatrix.Forward + scaleUp) >= 0 &&
aimVector.Dot(lidarWorldMatrix.Forward - scaleUp) >= 0)
{
return lidar;
}
}
else
{
if (aimVector.Dot(lidarWorldMatrix.Forward + scaleLeft) >= 0 ||
aimVector.Dot(lidarWorldMatrix.Forward - scaleLeft) >= 0 ||
aimVector.Dot(lidarWorldMatrix.Forward + scaleUp) >= 0 ||
aimVector.Dot(lidarWorldMatrix.Forward - scaleUp) >= 0)
{
return lidar;
}
}
}
}
}

return null;
}

bool IsValidLidarTarget(ref MyDetectedEntityInfo entityInfo, ref MatrixD referenceWorldMatrix)
{
if (entityInfo.Type != MyDetectedEntityType.Asteroid && entityInfo.Type != MyDetectedEntityType.Planet)
{
if (Vector3D.Distance(entityInfo.Position, referenceWorldMatrix.Translation) > LIDAR_MIN_LOCK_DISTANCE)
{
if (!excludeFriendly || IsNotFriendly(entityInfo.Relationship))
{
if (notMissile == null || (entityInfo.Position - ComputeBlockGridMidPoint(notMissile)).Length() > notMissileRadius)
{
if ((entityInfo.Position - referenceWorldMatrix.Translation).Length() >= LIDAR_MIN_LOCK_DISTANCE && (GetMissileMidPoint() - entityInfo.Position).Length() >= 1)
{
return true;
}
}
}
}
}
return false;
}

bool IsNotFriendly(VRage.Game.MyRelationsBetweenPlayerAndBlock relationship)
{
return (relationship != VRage.Game.MyRelationsBetweenPlayerAndBlock.FactionShare && relationship != VRage.Game.MyRelationsBetweenPlayerAndBlock.Owner);
}

//------------------------------ Missile Aiming Methods ------------------------------

int GetMultiplierSign(double value)
{
return (value < 0 ? -1 : 1);
}

void AimAtTarget()
{
//---------- Activate Gyroscopes To Turn Towards Target ----------

Vector3D yawVector = new Vector3D(targetVector.GetDim(0), 0, targetVector.GetDim(2));
Vector3D pitchVector = new Vector3D(0, targetVector.GetDim(1), targetVector.GetDim(2));
yawVector.Normalize();
pitchVector.Normalize();

targetYawAngle = Math.Acos(yawVector.Dot(Z_VECTOR)) * GetMultiplierSign(targetVector.GetDim(0));
targetPitchAngle = Math.Acos(pitchVector.Dot(Z_VECTOR)) * GetMultiplierSign(targetVector.GetDim(1));

double targetYawAngleLN = Math.Round(targetYawAngle, 2);
double targetPitchAngleLN = Math.Round(targetPitchAngle, 2);

//---------- PID Controller Adjustment ----------

lastYawIntegral = lastYawIntegral + (targetYawAngle / 60);
lastYawIntegral = (INTEGRAL_WINDUP_LIMIT > 0 ? Math.Max(Math.Min(lastYawIntegral, INTEGRAL_WINDUP_LIMIT), -INTEGRAL_WINDUP_LIMIT) : lastYawIntegral);
double yawDerivative = (targetYawAngleLN - lastYawError) * 60;
lastYawError = targetYawAngleLN;
targetYawAngle = (AIM_P * targetYawAngle) + (AIM_I * lastYawIntegral) + (AIM_D * yawDerivative);

lastPitchIntegral = lastPitchIntegral + (targetPitchAngle / 60);
lastPitchIntegral = (INTEGRAL_WINDUP_LIMIT > 0 ? Math.Max(Math.Min(lastPitchIntegral, INTEGRAL_WINDUP_LIMIT), -INTEGRAL_WINDUP_LIMIT) : lastPitchIntegral);
double pitchDerivative = (targetPitchAngleLN - lastPitchError) * 60;
lastPitchError = targetPitchAngleLN;
targetPitchAngle = (AIM_P * targetPitchAngle) + (AIM_I * lastPitchIntegral) + (AIM_D * pitchDerivative);

if (Math.Abs(targetYawAngle) + Math.Abs(targetPitchAngle) > AIM_LIMIT)
{
double adjust = AIM_LIMIT / (Math.Abs(targetYawAngle) + Math.Abs(targetPitchAngle));
targetYawAngle *= adjust;
targetPitchAngle *= adjust;
}

//---------- Set Gyroscope Parameters ----------

SetGyroYaw(targetYawAngle);
SetGyroPitch(targetPitchAngle);
}

void AimAtNaturalGravity()
{
//---------- Activate Gyroscopes To Aim Dampener At Natural Gravity ----------

if (refDownwardBlock == null || naturalGravityLength < 0.01)
{
return;
}

MatrixD dampenerLookAtMatrix = MatrixD.CreateLookAt(POINT_ZERO, refDownwardBlock.WorldMatrix.Forward, (refForwardReverse ? refWorldMatrix.Backward : refWorldMatrix.Forward));

Vector3D gravityVector = Vector3D.TransformNormal(naturalGravity, dampenerLookAtMatrix);
gravityVector.SetDim(1, 0);
gravityVector.Normalize();

if (Double.IsNaN(gravityVector.Sum))
{
gravityVector = new Vector3D(Z_VECTOR);
}

targetRollAngle = Math.Acos(gravityVector.Dot(Z_VECTOR)) * GetMultiplierSign(gravityVector.GetDim(0));

double targetRollAngleLN = Math.Round(targetRollAngle, 2);

//---------- PID Controller Adjustment ----------

lastRollIntegral = lastRollIntegral + (targetRollAngle / 60);
lastRollIntegral = (INTEGRAL_WINDUP_LIMIT > 0 ? Math.Max(Math.Min(lastRollIntegral, INTEGRAL_WINDUP_LIMIT), -INTEGRAL_WINDUP_LIMIT) : lastRollIntegral);
double rollDerivative = (targetRollAngleLN - lastRollError) * 60;
lastRollError = targetRollAngleLN;
targetRollAngle = (AIM_P * targetRollAngle) + (AIM_I * lastRollIntegral) + (AIM_D * rollDerivative);

//---------- Set Gyroscope Parameters ----------

SetGyroRoll(targetRollAngle);
}

//------------------------------ Missile Separation Methods ------------------------------

bool DetachFromGrid()
{
List<IMyTerminalBlock> blocks;

switch (missileDetachPortType)
{
case 0:
case 3:
blocks = (strDetachPortTag != null && strDetachPortTag.Length > 0 ? GetBlocksWithName<IMyShipMergeBlock>(strDetachPortTag) : GetBlocksOfType<IMyShipMergeBlock>());
detachBlock = GetClosestBlockFromReference(blocks, Me);

if (detachBlock == null)
{
Echo("Error: Cannot find any Merge Block " + (strDetachPortTag != null && strDetachPortTag.Length > 0 ? "with tag " + strDetachPortTag + " to detach" : "to detach."));
return false;
}
detachBlockType = 0;

detachBlock.ApplyAction("OnOff_Off");
return true;
case 1:
case 4:
blocks = (strDetachPortTag != null && strDetachPortTag.Length > 0 ? GetBlocksWithName<IMyMechanicalConnectionBlock>(strDetachPortTag) : GetBlocksOfType<IMyMechanicalConnectionBlock>());
detachBlock = GetClosestBlockFromReference(blocks, Me);

if (detachBlock == null)
{
Echo("Error: Cannot find any Rotor " + (strDetachPortTag != null && strDetachPortTag.Length > 0 ? "with tag " + strDetachPortTag + " to detach" : "to detach."));
return false;
}
detachBlockType = 1;

detachBlock.ApplyAction("Detach");
return true;
case 2:
blocks = (strDetachPortTag != null && strDetachPortTag.Length > 0 ? GetBlocksWithName<IMyShipConnector>(strDetachPortTag) : GetBlocksOfType<IMyShipConnector>());
detachBlock = GetClosestBlockFromReference(blocks, Me, true);

if (detachBlock == null)
{
Echo("Error: Cannot find any Connector " + (strDetachPortTag != null && strDetachPortTag.Length > 0 ? "with tag " + strDetachPortTag + " to detach" : "to detach."));
return false;
}
detachBlockType = 2;

detachBlock.ApplyAction("Unlock");
return true;
case 99:
return true;
default:
Echo("Error: Unknown missileDetachPortType - " + missileDetachPortType + ".");
return false;
}
}

IMyTerminalBlock GetClosestBlockFromReference(List<IMyTerminalBlock> checkBlocks, IMyTerminalBlock referenceBlock, bool sameGridCheck = false)
{
IMyTerminalBlock checkBlock = null;
double prevCheckDistance = Double.MaxValue;

for (int i = 0; i < checkBlocks.Count; i++)
{
if (!sameGridCheck || checkBlocks[i].CubeGrid == referenceBlock.CubeGrid)
{
double currCheckDistance = (checkBlocks[i].GetPosition() - referenceBlock.GetPosition()).Length();
if (currCheckDistance < prevCheckDistance)
{
prevCheckDistance = currCheckDistance;
checkBlock = checkBlocks[i];
}
}
}

return checkBlock;
}

IMyShipMergeBlock GetConnectedMergeBlock(IMyCubeGrid grid, IMyTerminalBlock mergeBlock)
{
IMySlimBlock slimBlock = grid.GetCubeBlock(mergeBlock.Position - new Vector3I(Base6Directions.GetVector(mergeBlock.Orientation.Left)));
return (slimBlock == null ? null : slimBlock.FatBlock as IMyShipMergeBlock);
}

void DetachLockedConnectors()
{
List<IMyTerminalBlock> blocks = GetBlocksOfType<IMyShipConnector>();
for (int i = 0; i < blocks.Count; i++)
{
if (blocks[i].CubeGrid == Me.CubeGrid)
{
IMyShipConnector otherConnector = ((IMyShipConnector)blocks[i]).OtherConnector;
if (otherConnector == null || blocks[i].CubeGrid != otherConnector.CubeGrid)
{
blocks[i].ApplyAction("Unlock");
}
}
}
}

//------------------------------ String Parsing Methods ------------------------------

bool ParseMatrix(string[] tokens, out MatrixD parsedMatrix, int start = 0, bool isOrientation = false)
{
if (tokens.Length < start + (isOrientation ? 9 : 16))
{
parsedMatrix = new MatrixD();
return false;
}

double v;
double[] r = new double[isOrientation ? 9 : 16];

for (int i = start; i < start + r.Length; i++)
{
if (Double.TryParse(tokens[i], out v))
{
r[i] = v;
}
}

if (isOrientation)
{
parsedMatrix = new MatrixD(r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7], r[8]);
}
else
{
parsedMatrix = new MatrixD(r[0], r[1], r[2], r[3], r[4], r[5], r[6], r[7], r[8], r[9], r[10], r[11], r[12], r[13], r[14], r[15]);
}

return true;
}

bool ParseVector(string[] tokens, out Vector3D parsedVector, int start = 0)
{
parsedVector = new Vector3D();

if (tokens.Length < start + 3)
{
return false;
}

double result;

if (Double.TryParse(tokens[start], out result))
{
parsedVector.SetDim(0, result);
}
else
{
return false;
}

if (Double.TryParse(tokens[start + 1], out result))
{
parsedVector.SetDim(1, result);
}
else
{
return false;
}

if (Double.TryParse(tokens[start + 2], out result))
{
parsedVector.SetDim(2, result);
}
else
{
return false;
}

return true;
}

bool ParseCoordinates(string coordinates, out Vector3D parsedVector)
{
parsedVector = new Vector3D();
coordinates = coordinates.Trim();

double result;
string[] tokens = coordinates.Split(':');

if (coordinates.StartsWith("GPS") && tokens.Length >= 5)
{
if (Double.TryParse(tokens[2], out result))
{
parsedVector.SetDim(0, result);
}
else
{
return false;
}

if (Double.TryParse(tokens[3], out result))
{
parsedVector.SetDim(1, result);
}
else
{
return false;
}

if (Double.TryParse(tokens[4], out result))
{
parsedVector.SetDim(2, result);
}
else
{
return false;
}

return true;
}
else if (coordinates.StartsWith("[T:") && tokens.Length >= 4)
{
if (Double.TryParse(tokens[1], out result))
{
parsedVector.SetDim(0, result);
}
else
{
return false;
}

if (Double.TryParse(tokens[2], out result))
{
parsedVector.SetDim(1, result);
}
else
{
return false;
}

if (Double.TryParse(tokens[3].Substring(0, tokens[3].Length - 1), out result))
{
parsedVector.SetDim(2, result);
}
else
{
return false;
}

return true;
}
else
{
return false;
}
}

//------------------------------ Command Processing Methods ------------------------------

void ProcessCustomConfiguration()
{
CustomConfiguration cfg = new CustomConfiguration(Me);
cfg.Load();

cfg.Get("missileLaunchType", ref missileLaunchType);
cfg.Get("missileDetachPortType", ref missileDetachPortType);
cfg.Get("spinAmount", ref spinAmount);
cfg.Get("verticalTakeoff", ref verticalTakeoff);
cfg.Get("waitForHomingTrigger", ref waitForHomingTrigger);
cfg.Get("enableMissileCommand", ref enableMissileCommand);
cfg.Get("missileId", ref missileId);
cfg.Get("missileGroup", ref missileGroup);
cfg.Get("allowedSenderId", ref allowedSenderId);
cfg.Get("strShipRefLidar", ref strShipRefLidar);
cfg.Get("strShipRefForward", ref strShipRefForward);
cfg.Get("strShipRefTargetPanel", ref strShipRefTargetPanel);
cfg.Get("strShipRefNotMissileTag", ref strShipRefNotMissileTag);
cfg.Get("missileActivationCommands", ref missileActivationCommands);
cfg.Get("missileTriggerCommands", ref missileTriggerCommands);
cfg.Get("strGyroscopesTag", ref strGyroscopesTag);
cfg.Get("strThrustersTag", ref strThrustersTag);
cfg.Get("strDetachPortTag", ref strDetachPortTag);
cfg.Get("strDirectionRefBlockTag", ref strDirectionRefBlockTag);
cfg.Get("strLockTriggerBlockTag", ref strLockTriggerBlockTag);
cfg.Get("strLockTriggerAction", ref strLockTriggerAction);
cfg.Get("strStatusDisplayPrefix", ref strStatusDisplayPrefix);
cfg.Get("driftVectorReduction", ref driftVectorReduction);
cfg.Get("launchSeconds", ref launchSeconds);
cfg.Get("boolDrift", ref boolDrift);
cfg.Get("boolLeadTarget", ref boolLeadTarget);
cfg.Get("boolNaturalDampener", ref boolNaturalDampener);
cfg.Get("LIDAR_MIN_LOCK_DISTANCE", ref LIDAR_MIN_LOCK_DISTANCE);
cfg.Get("LIDAR_MAX_LOCK_DISTANCE", ref LIDAR_MAX_LOCK_DISTANCE);
cfg.Get("LIDAR_REFRESH_INTERVAL", ref LIDAR_REFRESH_INTERVAL);
cfg.Get("excludeFriendly", ref excludeFriendly);
cfg.Get("DEF_SMALL_GRID_P", ref DEF_SMALL_GRID_P);
cfg.Get("DEF_SMALL_GRID_I", ref DEF_SMALL_GRID_I);
cfg.Get("DEF_SMALL_GRID_D", ref DEF_SMALL_GRID_D);
cfg.Get("DEF_BIG_GRID_P", ref DEF_BIG_GRID_P);
cfg.Get("DEF_BIG_GRID_I", ref DEF_BIG_GRID_I);
cfg.Get("DEF_BIG_GRID_D", ref DEF_BIG_GRID_D);
cfg.Get("useDefaultPIDValues", ref useDefaultPIDValues);
cfg.Get("AIM_P", ref AIM_P);
cfg.Get("AIM_I", ref AIM_I);
cfg.Get("AIM_D", ref AIM_D);
cfg.Get("AIM_LIMIT", ref AIM_LIMIT);
cfg.Get("INTEGRAL_WINDUP_LIMIT", ref INTEGRAL_WINDUP_LIMIT);
cfg.Get("MERGE_SEPARATE_WAIT_THRESHOLD", ref MERGE_SEPARATE_WAIT_THRESHOLD);
cfg.Get("TURRET_AI_PN_CONSTANT", ref TURRET_AI_PN_CONSTANT);
cfg.Get("TURRET_AI_AVERAGE_SIZE", ref TURRET_AI_AVERAGE_SIZE);
cfg.Get("outputMissileStatus", ref outputMissileStatus);
}

void ProcessConfigurationCommand(string commandLine)
{
string[] keyValues = commandLine.Split(',');

for (int i = 0; i < keyValues.Length; i++)
{
string[] tokens = keyValues[i].Trim().Split(':');
if (tokens.Length > 0)
{
ProcessSingleConfigCommand(tokens);
}
}
}

void ProcessSingleConfigCommand(string[] tokens)
{
string cmdToken = tokens[0].Trim();
if (cmdToken.Equals("MODE") && tokens.Length >= 2)
{
int modeValue;
if (Int32.TryParse(tokens[1], out modeValue))
{
missileLaunchType = modeValue;
}
}
else if (cmdToken.Equals("R_LDR") && tokens.Length >= 2)
{
strShipRefLidar = tokens[1];
}
else if (cmdToken.Equals("R_TAR") && tokens.Length >= 2)
{
strShipRefTargetPanel = tokens[1];
}
else if (cmdToken.Equals("R_FWD") && tokens.Length >= 2)
{
strShipRefForward = tokens[1];
}
else if (cmdToken.Equals("V_DVR") && tokens.Length >= 2)
{
double dvrValue;
if (Double.TryParse(tokens[1], out dvrValue))
{
driftVectorReduction = dvrValue;
}
}
else if (cmdToken.Equals("V_LS") && tokens.Length >= 2)
{
double lsValue;
if (Double.TryParse(tokens[1], out lsValue))
{
launchSeconds = lsValue;
}
}
else if (cmdToken.Equals("V_DRIFT") && tokens.Length >= 2)
{
bool driftValue;
if (bool.TryParse(tokens[1], out driftValue))
{
boolDrift = driftValue;
}
}
else if (cmdToken.Equals("V_LEAD") && tokens.Length >= 2)
{
bool leadValue;
if (bool.TryParse(tokens[1], out leadValue))
{
boolLeadTarget = leadValue;
}
}
else if (cmdToken.Equals("V_DAMP") && tokens.Length >= 2)
{
bool dampenerValue;
if (bool.TryParse(tokens[1], out dampenerValue))
{
boolNaturalDampener = dampenerValue;
}
}
else if (cmdToken.Equals("P_VT") && tokens.Length >= 2)
{
bool vtValue;
if (bool.TryParse(tokens[1], out vtValue))
{
verticalTakeoff = vtValue;
}
}
else if (cmdToken.Equals("P_WFT") && tokens.Length >= 2)
{
bool wftValue;
if (bool.TryParse(tokens[1], out wftValue))
{
waitForHomingTrigger = wftValue;
}
}
else if (cmdToken.Equals("P_EMC") && tokens.Length >= 2)
{
bool emcValue;
if (bool.TryParse(tokens[1], out emcValue))
{
enableMissileCommand = emcValue;
}
}
else if (cmdToken.Equals("SPIN") && tokens.Length >= 2)
{
double spinValue;
if (Double.TryParse(tokens[1], out spinValue))
{
spinAmount = (int)spinValue;
}
}
else if (cmdToken.Equals("CHECK"))
{
subMode = 0;

CheckScript();
}
}

void ProcessTriggerCommands()
{
if (rpmTriggerList != null && rpmTriggerList.Count > 0)
{
int i = 0;
while (i < rpmTriggerList.Count)
{
if (rpmTriggerList[i].Key <= rpm)
{
ProcessSingleMissileCommand(rpmTriggerList[i].Value);
rpmTriggerList.RemoveAt(i);
}
else
{
i++;
}
}
}

if (distTriggerList != null && distTriggerList.Count > 0)
{
int i = 0;
while (i < distTriggerList.Count)
{
if (distTriggerList[i].Key >= distToTarget)
{
ProcessSingleMissileCommand(distTriggerList[i].Value);
distTriggerList.RemoveAt(i);
}
else
{
i++;
}
}
}

if (timeTriggerList != null && timeTriggerList.Count > 0)
{
int i = 0;
while (i < timeTriggerList.Count)
{
if (timeTriggerList[i].Key <= clock)
{
ProcessSingleMissileCommand(timeTriggerList[i].Value);
timeTriggerList.RemoveAt(i);
}
else
{
i++;
}
}
}
}

void ProcessCommunicationMessage(string message)
{
string[] msgTokens = message.Split(new char[] {'\r','\n'}, StringSplitOptions.RemoveEmptyEntries);

for (int i = 0; i < msgTokens.Length; i++)
{
string msg = msgTokens[i];

string recipient;
string sender;
string options;     //Not Supported Yet, For Future Use

int start = msg.IndexOf("MSG;", 0, StringComparison.OrdinalIgnoreCase);
if (start > -1)
{
start += 4;

recipient = NextToken(msg, ref start, ';');
sender = NextToken(msg, ref start, ';');
options = NextToken(msg, ref start, ';');

if (IsValidRecipient(recipient) && IsValidSender(sender))
{
if (msg.Length > start)
{
ProcessMissileCommand(msg.Substring(start));
}
}
}
}
}

bool IsValidRecipient(string recipient)
{
if (recipient.Length == 0)
{
return true;
}

int code = (recipient[0] == '*' ? 1 : 0) + (recipient[recipient.Length - 1] == '*' ? 2 : 0);
switch (code)
{
case 0:
return missileId.Equals(recipient, StringComparison.OrdinalIgnoreCase) ||
(missileGroup != null && missileGroup.Equals(recipient, StringComparison.OrdinalIgnoreCase));
case 1:
return missileId.EndsWith(recipient.Substring(1), StringComparison.OrdinalIgnoreCase) ||
(missileGroup != null && missileGroup.EndsWith(recipient.Substring(1), StringComparison.OrdinalIgnoreCase));
case 2:
return missileId.StartsWith(recipient.Substring(0, recipient.Length - 1), StringComparison.OrdinalIgnoreCase) ||
(missileGroup != null && missileGroup.StartsWith(recipient.Substring(0, recipient.Length - 1), StringComparison.OrdinalIgnoreCase));
default:
return (recipient.Length == 1) || (missileId.IndexOf(recipient.Substring(1, recipient.Length - 2), StringComparison.OrdinalIgnoreCase) > -1) ||
(missileGroup != null && (missileGroup.IndexOf(recipient.Substring(1, recipient.Length - 2), StringComparison.OrdinalIgnoreCase) > -1));
}
}

bool IsValidSender(string sender)
{
if (allowedSenderId == null || allowedSenderId.Length == 0)
{
return true;
}

int code = (allowedSenderId[0] == '*' ? 1 : 0) + (allowedSenderId[allowedSenderId.Length - 1] == '*' ? 2 : 0);
switch (code)
{
case 0:
return sender.Equals(allowedSenderId, StringComparison.OrdinalIgnoreCase);
case 1:
return sender.EndsWith(allowedSenderId.Substring(1), StringComparison.OrdinalIgnoreCase);
case 2:
return sender.StartsWith(allowedSenderId.Substring(0, allowedSenderId.Length - 1), StringComparison.OrdinalIgnoreCase);
default:
return (allowedSenderId.Length == 1) || (sender.IndexOf(allowedSenderId.Substring(1, allowedSenderId.Length - 2), StringComparison.OrdinalIgnoreCase) > -1);
}
}

string NextToken(string line, ref int start, char delim)
{
if (line.Length > start)
{
int end = line.IndexOf(delim, start);
if (end > -1)
{
string result = line.Substring(start, end - start);
start = end + 1;
return result;
}
}
start = line.Length;
return "";
}

void ProcessMissileCommand(string commandLine)
{
string[] keyValues = commandLine.Split(',');

for (int i = 0; i < keyValues.Length; i++)
{
string[] tokens = keyValues[i].Trim().Split(':');
if (tokens.Length > 0)
{
ProcessSingleMissileCommand(tokens);
}
}
}

void ProcessSingleMissileCommand(string[] tokens)
{
//Additional Inter-Grid Communication Commands
//    LOCK  -  Set homingReleaseLock to true
//    GPS:<X>:<Y>:<Z>  -  Set targetPosition
//    FWD:<A>:<B>:<C>:<X>:<Y>:<Z>  -  Set forwardPosition. ABC is forward vector, XYZ is startingPoint
//    LDR:<ID>:...  -  Set lidarEntityInfo => 1 : Entity ID, 2-4 : Target Position, 5-7 : Target Velocity,
//                                            8-10 : Hit Position, 11-13 : Target Bounding Box Mininum, 14-22 : Orientation Matrix,
//                                            23 : Target Type, 24 : Target Relationship, 25 : Timestamp, 26 : Entity Name

string cmdToken = tokens[0].Trim().ToUpper();
if (cmdToken.Equals("GPS"))
{
if (tokens.Length >= 4)
{
Vector3D parsedVector;
if (ParseVector(tokens, out parsedVector, (tokens.Length == 4 ? 1 : 2)))
{
commsPosition = parsedVector;
commsPositionSet = true;
}
}
}
else if (cmdToken.Equals("FWD"))
{
if (tokens.Length >= 4)
{
Vector3D parsedVector1;
if (ParseVector(tokens, out parsedVector1, 1))
{
Vector3D parsedVector2;
if (tokens.Length >= 7)
{
if (!ParseVector(tokens, out parsedVector2, 4))
{
parsedVector2 = new Vector3D();
}
}
else
{
parsedVector2 = new Vector3D();
}
commsForward = new RayD(parsedVector2, parsedVector1);
commsForwardSet = true;
}
}
}
else if (cmdToken.Equals("LDR"))
{
if (tokens.Length >= 2)
{
long entityId;
if (!long.TryParse(tokens[1], out entityId))
{
entityId = -1;
}

Vector3D position;
if (!(tokens.Length >= 5 && ParseVector(tokens, out position, 2)))
{
position = new Vector3D();
}

Vector3D velocity;
if (!(tokens.Length >= 8 && ParseVector(tokens, out velocity, 5)))
{
velocity = new Vector3D();
}

Vector3D hitPosition;
if (!(tokens.Length >= 11 && ParseVector(tokens, out hitPosition, 8)))
{
hitPosition = position;
}

Vector3D boxMin;
if (!(tokens.Length >= 14 && ParseVector(tokens, out boxMin, 11)))
{
boxMin = position + new Vector3D(-1.25, -1.25, -1.25);
}

MatrixD orientation;
if (!(tokens.Length >= 23 && ParseMatrix(tokens, out orientation, 14, true)))
{
orientation = new MatrixD();
}

int value;
MyDetectedEntityType targetType;
if (!(tokens.Length >= 24 && !int.TryParse(tokens[23], out value)))
{
value = 3;
}
try { targetType = (MyDetectedEntityType)value; }
catch { targetType = MyDetectedEntityType.LargeGrid; }

VRage.Game.MyRelationsBetweenPlayerAndBlock targetRelationship;
if (!(tokens.Length >= 25 && !int.TryParse(tokens[24], out value)))
{
value = 3;
}
try { targetRelationship = (VRage.Game.MyRelationsBetweenPlayerAndBlock)value; }
catch { targetRelationship = VRage.Game.MyRelationsBetweenPlayerAndBlock.Neutral; }

long timestamp;
if (!(tokens.Length >= 26 && !long.TryParse(tokens[25], out timestamp)))
{
timestamp = DateTime.Now.Ticks;
}
try { targetRelationship = (VRage.Game.MyRelationsBetweenPlayerAndBlock)value; }
catch { targetRelationship = VRage.Game.MyRelationsBetweenPlayerAndBlock.Neutral; }

BoundingBoxD boundingBox = new BoundingBoxD(boxMin, position + position - boxMin);

commsLidarTarget = new MyDetectedEntityInfo(entityId, (tokens.Length >= 27 ? tokens[26] : ""), targetType, hitPosition, orientation, velocity, targetRelationship, boundingBox, timestamp);
commsLidarTargetSet = true;
}
}
else if (cmdToken.Equals("LOCK"))
{
homingReleaseLock = true;
}
else if (cmdToken.Equals("ABORT"))
{
ZeroTurnGyro();

mode = 99;
}
else if (cmdToken.Equals("SEVER"))
{
if (shipRefLidars != null)
{
shipRefLidars.Clear();
}

shipRefForward = null;
shipRefTargetPanel = null;

enableMissileCommand = false;
}
else if (cmdToken.Equals("CRUISE"))
{
if (verticalTakeoff)
{
FireThrusters(launchThrusters, false);
FireThrusters(thrusters, true);
}

ResetGyro();
SetGyroOverride(true);

if (spinAmount > 0)
{
SetGyroRoll(spinAmount);
}

lastTargetPosition = targetPosition = GetFlyStraightVector();

subCounter = 0;
subMode = 0;
mode = 4;
}
else if (cmdToken.Equals("GLIDE"))
{
if (verticalTakeoff)
{
FireThrusters(launchThrusters, false);
FireThrusters(thrusters, true);
}

ResetGyro();
SetGyroOverride(true);

if (spinAmount > 0)
{
SetGyroRoll(spinAmount);
}

//TODO: Change this line based on the existing missile mode and subMode state
if (mode == -1)
{
refForwardPosition = midPoint;
refForwardVector = Vector3D.Normalize(driftVector);
refForwardSet = true;
}
else if (mode == 8 && homingTurret != null)
{
refForwardPosition = midPoint;
refForwardVector = CalculateTurretViewVector(homingTurret);
refForwardSet = true;
}
else if ((mode == 2 && subMode == 0) || (mode == 3))
{
if (commsForwardSet)
{
commsForwardSet = false;

refForwardPosition = commsForward.Position;
refForwardVector = commsForward.Direction;
refForwardSet = true;
}
else if (shipRefForward != null)
{
refForwardPosition = shipRefForward.WorldMatrix.Translation;
refForwardVector = (forwardIsTurret ? CalculateTurretViewVector(shipRefForward as IMyLargeTurretBase) : shipRefForward.WorldMatrix.Forward);
refForwardSet = true;
}
}
else
{
refForwardPosition = midPoint;
refForwardVector = Vector3D.Normalize(targetPosition - midPoint);
refForwardSet = true;
}

subCounter = 0;
subMode = 0;
mode = 6;
}
else if (cmdToken.StartsWith("ACT") && tokens.Length >= 3)
{
char opCode = (cmdToken.Length >= 4 ? cmdToken[3] : 'B');
List<IMyTerminalBlock> triggerBlocks = null;
switch (opCode)
{
case 'B':
triggerBlocks = GetBlocksWithName<IMyTerminalBlock>(tokens[1], 3);
break;
case 'P':
triggerBlocks = GetBlocksWithName<IMyTerminalBlock>(tokens[1], 1);
break;
case 'S':
triggerBlocks = GetBlocksWithName<IMyTerminalBlock>(tokens[1], 2);
break;
case 'W':
triggerBlocks = GetBlocksWithName<IMyTerminalBlock>(tokens[1], 0);
break;
}

if (triggerBlocks != null)
{
for (int i = 0; i < triggerBlocks.Count; i++)
{
ITerminalAction action = triggerBlocks[i].GetActionWithName(tokens[2]);
if (action != null)
{
action.Apply(triggerBlocks[i]);
}
}
}
}
else if (cmdToken.StartsWith("SET") && tokens.Length >= 3)
{
char opCode = (cmdToken.Length >= 4 ? cmdToken[3] : 'B');
List<IMyTerminalBlock> triggerBlocks = null;
switch (opCode)
{
case 'B':
triggerBlocks = GetBlocksWithName<IMyTerminalBlock>(tokens[1], 3);
break;
case 'P':
triggerBlocks = GetBlocksWithName<IMyTerminalBlock>(tokens[1], 1);
break;
case 'S':
triggerBlocks = GetBlocksWithName<IMyTerminalBlock>(tokens[1], 2);
break;
case 'W':
triggerBlocks = GetBlocksWithName<IMyTerminalBlock>(tokens[1], 0);
break;
}

char propCode = (cmdToken.Length >= 5 ? cmdToken[4] : 'P');

if (triggerBlocks != null)
{
for (int i = 0; i < triggerBlocks.Count; i++)
{
switch (propCode)
{
case 'P':
triggerBlocks[i].SetValueFloat(tokens[2], float.Parse(tokens[3]));
break;
case 'B':
triggerBlocks[i].SetValueBool(tokens[2], bool.Parse(tokens[3]));
break;
case 'D':
triggerBlocks[i].SetValueFloat(tokens[2], (float)distToTarget / float.Parse(tokens[3]));
break;
case 'S':
triggerBlocks[i].SetValueFloat(tokens[2], (float)speed / float.Parse(tokens[3]));
break;
case 'T':
triggerBlocks[i].SetValueFloat(tokens[2], (float)(distToTarget / speed) / float.Parse(tokens[3]));
break;
case 'A':
triggerBlocks[i].SetValueFloat(tokens[2], triggerBlocks[i].GetValueFloat(tokens[2]) + float.Parse(tokens[3]));
break;
case 'M':
triggerBlocks[i].SetValueFloat(tokens[2], triggerBlocks[i].GetValueFloat(tokens[2]) * float.Parse(tokens[3]));
break;
}
}
}
}
else if (cmdToken.Equals("SPIN") && tokens.Length >= 1)
{
SetGyroRoll(tokens.Length >= 2 ? Int32.Parse(tokens[1]) : 30);
}
}

void ExecuteTriggerCommand(string commandLine)
{
int startIndex = commandLine.IndexOf('[') + 1;
int endIndex = commandLine.LastIndexOf(']');

string command = (startIndex > 0 && endIndex > -1 ? commandLine.Substring(startIndex, endIndex - startIndex) : commandLine);
string[] keyValues = command.Split(',');

for (int i = 0; i < keyValues.Length; i++)
{
string[] tokens = keyValues[i].Trim().Split(':');
if (tokens.Length > 0)
{
string cmdToken = tokens[0].Trim();
if (cmdToken.Equals("TGR") && tokens.Length >= 3)
{
if (rpmTriggerList == null)
{
rpmTriggerList = new List<KeyValuePair<double, string[]>>();
}

string[] items = new string[tokens.Length - 2];
Array.Copy(tokens, 2, items, 0, items.Length);
rpmTriggerList.Add(new KeyValuePair<double, string[]>(Double.Parse(tokens[1]), items));

haveTriggerCommands = true;
}
else if (cmdToken.Equals("TGD") && tokens.Length >= 3)
{
if (distTriggerList == null)
{
distTriggerList = new List<KeyValuePair<double, string[]>>();
}

string[] items = new string[tokens.Length - 2];
Array.Copy(tokens, 2, items, 0, items.Length);
distTriggerList.Add(new KeyValuePair<double, string[]>(Double.Parse(tokens[1]), items));

haveTriggerCommands = true;
}
else if (cmdToken.Equals("TGE") && tokens.Length >= 3)
{
if (distTriggerList == null)
{
distTriggerList = new List<KeyValuePair<double, string[]>>();
}

string[] items = new string[tokens.Length - 2];
Array.Copy(tokens, 2, items, 0, items.Length);
distTriggerList.Add(new KeyValuePair<double, string[]>(distToTarget - Double.Parse(tokens[1]), items));

haveTriggerCommands = true;
}
else if (cmdToken.Equals("TGT") && tokens.Length >= 3)
{
if (timeTriggerList == null)
{
timeTriggerList = new List<KeyValuePair<int, string[]>>();
}

string[] items = new string[tokens.Length - 2];
Array.Copy(tokens, 2, items, 0, items.Length);
int ticks = (int)(Double.Parse(tokens[1]) * 60) + clock;
timeTriggerList.Add(new KeyValuePair<int, string[]>(ticks, items));

haveTriggerCommands = true;
}
else
{
ProcessSingleMissileCommand(tokens);
}
}
}
}

//------------------------------ Script Debugging Methods ------------------------------

void CheckScript()
{
InitLaunchingShipRefBlocks();

Echo("--- End Of Check, Please Recompile Script And Remove CHECK Argument ---");
}

//------------------------------ Initialization Methods ------------------------------

bool InitLaunchingShipRefBlocks()
{
List<IMyTerminalBlock> blocks;

blocks = GetBlocksWithName<IMyCameraBlock>(strShipRefLidar);
if (blocks.Count == 0)
{
Echo("Warning: Camera Lidars with tag " + strShipRefLidar + " not found.");

shipRefLidars = new List<IMyCameraBlock>(1);
}
else
{
shipRefLidars = new List<IMyCameraBlock>(blocks.Count);

for (int i = 0; i < blocks.Count; i++)
{
IMyCameraBlock refLidar = blocks[i] as IMyCameraBlock;

refLidar.ApplyAction("OnOff_On");
refLidar.EnableRaycast = true;

shipRefLidars.Add(refLidar);
}
}

blocks = GetBlocksWithName<IMyTextPanel>(strShipRefTargetPanel);
if (blocks.Count == 0)
{
Echo("Warning: Text Panel with tag " + strShipRefTargetPanel + " not found.");
}
else
{
if (blocks.Count > 1)
{
Echo("Warning: More than one Text Panel with tag " + strShipRefTargetPanel + " found. Using first one detected.");
}

shipRefTargetPanel = blocks[0] as IMyTextPanel;
}

if (shipRefTargetPanel == null)
{
enableMissileCommand = false;
}

blocks = GetBlocksWithName<IMyTerminalBlock>(strShipRefForward);
if (blocks.Count == 0)
{
blocks = GetBlocksOfType<IMyCockpit>();

if (blocks.Count == 0)
{
Echo("Warning: Forward Or Aiming Block with tag " + strShipRefForward + " not found.");
}
else
{
Echo("Warning: Forward Or Aiming Block with tag " + strShipRefForward + " not found. Using available Main Cockpit as reference instead.");

for (int i = 0; i < blocks.Count; i++)
{
shipRefForward = blocks[i];

if (shipRefForward.GetValueBool("MainCockpit"))
{
break;
}
}
}
}
else
{
if (blocks.Count > 1)
{
Echo("Warning: More than one Forward Or Aiming Block with tag " + strShipRefForward + " found. Using first one detected.");
}

shipRefForward = blocks[0];
}

forwardIsTurret = ((shipRefForward as IMyLargeTurretBase) != null);

alertBlock = GetSingleBlockWithName(strLockTriggerBlockTag);

notMissile = GetSingleBlockWithName(strShipRefNotMissileTag);

return true;
}

bool InitMissileBlocks()
{
gyroscopes = GetGyroscopes();

thrusters = GetThrusters();

remoteControl = GetRemoteControl();

missileLidars = GetLidars();

homingTurret = GetHomingTurret();
if (homingTurret != null)
{
homingTurret.EnableIdleRotation = false;
}

for (int i = 0; i < missileLidars.Count; i++)
{
missileLidars[i].ApplyAction("OnOff_On");
missileLidars[i].EnableRaycast = true;
}

if (missileLidars.Count > 0 || shipRefLidars.Count > 0)
{
IMyCameraBlock lidar = (missileLidars.Count > 0 ? missileLidars[0] : shipRefLidars[0]);

coneLimit = lidar.RaycastConeLimit;
sideScale = Math.Tan((90 - coneLimit) * RADIAN_FACTOR);
}

bool isFixedDirection = false;

if (strDirectionRefBlockTag != null && strDirectionRefBlockTag.Length > 0)
{
refForwardBlock = GetSingleBlockWithName(strDirectionRefBlockTag);
isFixedDirection = (refForwardBlock != null);
}

if (spinAmount > 0)
{
boolNaturalDampener = false;
}

if (refForwardBlock == null || boolNaturalDampener == null || boolDrift == null || verticalTakeoff)
{
thrustValues = ComputeMaxThrustValues(thrusters);
}

if (refForwardBlock == null)
{
refForwardBlock = ComputeHighestThrustReference(thrusters, thrustValues);
refForwardReverse = true;
}

if (refForwardBlock == null)
{
Echo("Warning: No Reference Blocks or Forward Thrusters found. Using Programmable Block as Reference Block.");

refForwardBlock = Me;
refForwardReverse = false;
}

refWorldMatrix = refForwardBlock.WorldMatrix;
if (refForwardReverse)
{
refWorldMatrix = MatrixD.CreateWorld(refWorldMatrix.Translation, refWorldMatrix.Backward, refWorldMatrix.Up);
}

InitGyrosAndThrusters(isFixedDirection);
thrustValues = null;

if (boolLeadTarget == null)
{
boolLeadTarget = true;
}

if (strStatusDisplayPrefix != null && strStatusDisplayPrefix.Length > 0)
{
List<IMyTerminalBlock> blocks = GetBlocksWithName<IMyTerminalBlock>(strStatusDisplayPrefix, 1);
if (blocks.Count > 0)
{
statusDisplay = blocks[0];

if (statusDisplay.HasAction("OnOff_On"))
{
statusDisplay.ApplyAction("OnOff_On");

IMyRadioAntenna radioAntenna = statusDisplay as IMyRadioAntenna;
if (radioAntenna != null && !radioAntenna.IsBroadcasting)
{
radioAntenna.ApplyAction("EnableBroadCast");
}
}
}
}

return true;
}

void TurnOnBlocks(List<IMyTerminalBlock> blocks)
{
for (int i = 0; i < blocks.Count; i++)
{
blocks[i].ApplyAction("OnOff_On");
}
}

List<IMyTerminalBlock> GetGyroscopes()
{
List<IMyTerminalBlock> blocks = GetBlocksWithName<IMyGyro>(strGyroscopesTag);
if (blocks.Count > 0)
{
return blocks;
}

GridTerminalSystem.GetBlocksOfType<IMyGyro>(blocks);
if (blocks.Count == 0)
{
Echo("Warning: No Gyroscopes found.");
}
return blocks;
}

List<IMyTerminalBlock> GetThrusters()
{
List<IMyTerminalBlock> blocks = GetBlocksWithName<IMyThrust>(strThrustersTag);
if (blocks.Count > 0)
{
return blocks;
}

GridTerminalSystem.GetBlocksOfType<IMyThrust>(blocks);
if (blocks.Count == 0)
{
Echo("Warning: No Thrusters found.");
}
return blocks;
}

IMyShipController GetRemoteControl()
{
List<IMyTerminalBlock> blocks = GetBlocksOfType<IMyShipController>();
IMyShipController remoteBlock = (blocks.Count > 0 ? blocks[0] as IMyShipController : null);
if (remoteBlock == null)
{
Echo("Error: No Remote Control found.");
}
return remoteBlock;
}

List<IMyCameraBlock> GetLidars()
{
return GetBlocksOfTypeCasted<IMyCameraBlock>();
}

IMyLargeTurretBase GetHomingTurret()
{
List<IMyTerminalBlock> blocks = GetBlocksOfType<IMyLargeTurretBase>();
IMyLargeTurretBase turret = (blocks.Count > 0 ? blocks[0] as IMyLargeTurretBase : null);

return turret;
}

void InitGyrosAndThrusters(bool isFixedDirection)
{
//---------- Find Gyroscope Orientation With Respect To Ship ----------

gyroYawField = new string[gyroscopes.Count];
gyroPitchField = new string[gyroscopes.Count];
gyroYawFactor = new float[gyroscopes.Count];
gyroPitchFactor = new float[gyroscopes.Count];
gyroRollField = new string[gyroscopes.Count];
gyroRollFactor = new float[gyroscopes.Count];

for (int i = 0; i < gyroscopes.Count; i++)
{
Base6Directions.Direction gyroUp = gyroscopes[i].WorldMatrix.GetClosestDirection(refWorldMatrix.Up);
Base6Directions.Direction gyroLeft = gyroscopes[i].WorldMatrix.GetClosestDirection(refWorldMatrix.Left);
Base6Directions.Direction gyroForward = gyroscopes[i].WorldMatrix.GetClosestDirection(refWorldMatrix.Forward);

switch (gyroUp)
{
case Base6Directions.Direction.Up:
gyroYawField[i] = "Yaw";
gyroYawFactor[i] = GYRO_FACTOR;
break;
case Base6Directions.Direction.Down:
gyroYawField[i] = "Yaw";
gyroYawFactor[i] = -GYRO_FACTOR;
break;
case Base6Directions.Direction.Left:
gyroYawField[i] = "Pitch";
gyroYawFactor[i] = GYRO_FACTOR;
break;
case Base6Directions.Direction.Right:
gyroYawField[i] = "Pitch";
gyroYawFactor[i] = -GYRO_FACTOR;
break;
case Base6Directions.Direction.Forward:
gyroYawField[i] = "Roll";
gyroYawFactor[i] = -GYRO_FACTOR;
break;
case Base6Directions.Direction.Backward:
gyroYawField[i] = "Roll";
gyroYawFactor[i] = GYRO_FACTOR;
break;
}

switch (gyroLeft)
{
case Base6Directions.Direction.Up:
gyroPitchField[i] = "Yaw";
gyroPitchFactor[i] = GYRO_FACTOR;
break;
case Base6Directions.Direction.Down:
gyroPitchField[i] = "Yaw";
gyroPitchFactor[i] = -GYRO_FACTOR;
break;
case Base6Directions.Direction.Left:
gyroPitchField[i] = "Pitch";
gyroPitchFactor[i] = GYRO_FACTOR;
break;
case Base6Directions.Direction.Right:
gyroPitchField[i] = "Pitch";
gyroPitchFactor[i] = -GYRO_FACTOR;
break;
case Base6Directions.Direction.Forward:
gyroPitchField[i] = "Roll";
gyroPitchFactor[i] = -GYRO_FACTOR;
break;
case Base6Directions.Direction.Backward:
gyroPitchField[i] = "Roll";
gyroPitchFactor[i] = GYRO_FACTOR;
break;
}

switch (gyroForward)
{
case Base6Directions.Direction.Up:
gyroRollField[i] = "Yaw";
gyroRollFactor[i] = GYRO_FACTOR;
break;
case Base6Directions.Direction.Down:
gyroRollField[i] = "Yaw";
gyroRollFactor[i] = -GYRO_FACTOR;
break;
case Base6Directions.Direction.Left:
gyroRollField[i] = "Pitch";
gyroRollFactor[i] = GYRO_FACTOR;
break;
case Base6Directions.Direction.Right:
gyroRollField[i] = "Pitch";
gyroRollFactor[i] = -GYRO_FACTOR;
break;
case Base6Directions.Direction.Forward:
gyroRollField[i] = "Roll";
gyroRollFactor[i] = -GYRO_FACTOR;
break;
case Base6Directions.Direction.Backward:
gyroRollField[i] = "Roll";
gyroRollFactor[i] = GYRO_FACTOR;
break;
}

gyroscopes[i].ApplyAction("OnOff_On");
}

//---------- Check Whether To Use Default PID Values ----------

if (useDefaultPIDValues)
{
if (Me.CubeGrid.ToString().Contains("Large"))
{
AIM_P = DEF_BIG_GRID_P;
AIM_I = DEF_BIG_GRID_I;
AIM_D = DEF_BIG_GRID_D;
}
else
{
AIM_P = DEF_SMALL_GRID_P;
AIM_I = DEF_SMALL_GRID_I;
AIM_D = DEF_SMALL_GRID_D;
AIM_LIMIT *= 2;
}
}

//---------- Find Forward Thrusters ----------

List<IMyTerminalBlock> checkThrusters = thrusters;
thrusters = new List<IMyTerminalBlock>();

if (!isFixedDirection || boolNaturalDampener == null || boolDrift == null || verticalTakeoff)
{
IMyTerminalBlock leftThruster = null;
IMyTerminalBlock rightThruster = null;
IMyTerminalBlock upThruster = null;
IMyTerminalBlock downThruster = null;

float leftThrustTotal = 0;
float rightThrustTotal = 0;
float upThrustTotal = 0;
float downThrustTotal = 0;

for (int i = 0; i < checkThrusters.Count; i++)
{
Base6Directions.Direction thrusterDirection = refWorldMatrix.GetClosestDirection(checkThrusters[i].WorldMatrix.Backward);
switch (thrusterDirection)
{
case Base6Directions.Direction.Forward:
thrusters.Add(checkThrusters[i]);
break;
case Base6Directions.Direction.Left:
leftThruster = checkThrusters[i];
leftThrustTotal += thrustValues[i];
break;
case Base6Directions.Direction.Right:
rightThruster = checkThrusters[i];
rightThrustTotal += thrustValues[i];
break;
case Base6Directions.Direction.Up:
upThruster = checkThrusters[i];
upThrustTotal += thrustValues[i];
if (isFixedDirection)
{
refDownwardBlock = upThruster;
}
break;
case Base6Directions.Direction.Down:
downThruster = checkThrusters[i];
downThrustTotal += thrustValues[i];
break;
}

checkThrusters[i].ApplyAction("OnOff_On");
}

float highestThrust = Math.Max(Math.Max(Math.Max(leftThrustTotal, rightThrustTotal), upThrustTotal), downThrustTotal);
if (highestThrust == 0)
{
if (boolNaturalDampener == true)
{
Echo("Warning: Natural Gravity Dampener feature not possible as there are no Downward Thrusters found.");
}
boolNaturalDampener = false;

if (boolDrift == null)
{
boolDrift = true;
}
}
else
{
if (!isFixedDirection)
{
if (leftThrustTotal == highestThrust)
{
refDownwardBlock = leftThruster;
}
else if (rightThrustTotal == highestThrust)
{
refDownwardBlock = rightThruster;
}
else if (upThrustTotal == highestThrust)
{
refDownwardBlock = upThruster;
}
else
{
refDownwardBlock = downThruster;
}
}
boolNaturalDampener = (refDownwardBlock != null);

if (boolDrift == null)
{
float lowestThrust = Math.Min(Math.Min(Math.Min(leftThrustTotal, rightThrustTotal), upThrustTotal), downThrustTotal);
boolDrift = (highestThrust > lowestThrust * 2);
}
}

if (verticalTakeoff && refDownwardBlock != null)
{
launchThrusters = new List<IMyTerminalBlock>();

for (int i = 0; i < checkThrusters.Count; i++)
{
if (refDownwardBlock.WorldMatrix.Forward.Dot(checkThrusters[i].WorldMatrix.Forward) >= 0.9)
{
launchThrusters.Add(checkThrusters[i]);
}
}
}
}
else
{
for (int i = 0; i < checkThrusters.Count; i++)
{
Base6Directions.Direction thrusterDirection = refWorldMatrix.GetClosestDirection(checkThrusters[i].WorldMatrix.Backward);
if (thrusterDirection == Base6Directions.Direction.Forward)
{
thrusters.Add(checkThrusters[i]);
}
else if (boolNaturalDampener == true && thrusterDirection == Base6Directions.Direction.Up)
{
refDownwardBlock = checkThrusters[i];
}

checkThrusters[i].ApplyAction("OnOff_On");
}

if (boolNaturalDampener == true && refDownwardBlock == null)
{
Echo("Warning: Natural Gravity Dampener feature not possible as there are no Downward Thrusters found.");
boolNaturalDampener = false;
}
}
}

float[] ComputeMaxThrustValues(List<IMyTerminalBlock> checkThrusters)
{
float[] thrustValues = new float[checkThrusters.Count];

for (int i = 0; i < checkThrusters.Count; i++)
{
thrustValues[i] = ((IMyThrust)checkThrusters[i]).MaxEffectiveThrust;
}

return thrustValues;
}

IMyTerminalBlock ComputeHighestThrustReference(List<IMyTerminalBlock> checkThrusters, float[] thrustValues)
{
IMyTerminalBlock fwdThruster = null;
IMyTerminalBlock bwdThruster = null;
IMyTerminalBlock leftThruster = null;
IMyTerminalBlock rightThruster = null;
IMyTerminalBlock upThruster = null;
IMyTerminalBlock downThruster = null;

float fwdThrustTotal = 0;
float bwdThrustTotal = 0;
float leftThrustTotal = 0;
float rightThrustTotal = 0;
float upThrustTotal = 0;
float downThrustTotal = 0;

for (int i = 0; i < checkThrusters.Count; i++)
{
Base6Directions.Direction thrusterDirection = Me.WorldMatrix.GetClosestDirection(checkThrusters[i].WorldMatrix.Backward);
switch (thrusterDirection)
{
case Base6Directions.Direction.Forward:
fwdThruster = checkThrusters[i];
fwdThrustTotal += thrustValues[i];
break;
case Base6Directions.Direction.Backward:
bwdThruster = checkThrusters[i];
bwdThrustTotal += thrustValues[i];
break;
case Base6Directions.Direction.Left:
leftThruster = checkThrusters[i];
leftThrustTotal += thrustValues[i];
break;
case Base6Directions.Direction.Right:
rightThruster = checkThrusters[i];
rightThrustTotal += thrustValues[i];
break;
case Base6Directions.Direction.Up:
upThruster = checkThrusters[i];
upThrustTotal += thrustValues[i];
break;
case Base6Directions.Direction.Down:
downThruster = checkThrusters[i];
downThrustTotal += thrustValues[i];
break;
}
}

List<IMyTerminalBlock> highestThrustReferences = new List<IMyTerminalBlock>(2);

float highestThrust = Math.Max(Math.Max(Math.Max(Math.Max(Math.Max(fwdThrustTotal, bwdThrustTotal), leftThrustTotal), rightThrustTotal), upThrustTotal), downThrustTotal);
if (fwdThrustTotal == highestThrust)
{
highestThrustReferences.Add(fwdThruster);
}
if (bwdThrustTotal == highestThrust)
{
highestThrustReferences.Add(bwdThruster);
}
if (leftThrustTotal == highestThrust)
{
highestThrustReferences.Add(leftThruster);
}
if (rightThrustTotal == highestThrust)
{
highestThrustReferences.Add(rightThruster);
}
if (upThrustTotal == highestThrust)
{
highestThrustReferences.Add(upThruster);
}
if (downThrustTotal == highestThrust)
{
highestThrustReferences.Add(downThruster);
}

if (highestThrustReferences.Count == 1)
{
return highestThrustReferences[0];
}
else
{
Vector3D diagonalVector = ComputeBlockGridDiagonalVector(Me);

IMyTerminalBlock closestToLengthRef = highestThrustReferences[0];
double closestToLengthValue = 0;

for (int i = 0; i < highestThrustReferences.Count; i++)
{
double dotLength = Math.Abs(diagonalVector.Dot(highestThrustReferences[i].WorldMatrix.Forward));
if (dotLength > closestToLengthValue)
{
closestToLengthValue = dotLength;
closestToLengthRef = highestThrustReferences[i];
}
}

return closestToLengthRef;
}
}

Vector3D ComputeBlockGridDiagonalVector(IMyTerminalBlock block)
{
IMyCubeGrid cubeGrid = block.CubeGrid;

Vector3D minVector = cubeGrid.GridIntegerToWorld(cubeGrid.Min);
Vector3D maxVector = cubeGrid.GridIntegerToWorld(cubeGrid.Max);

return (minVector - maxVector);
}

Vector3D ComputeBlockGridMidPoint(IMyTerminalBlock block)
{
return (block.CubeGrid.GridIntegerToWorld(block.CubeGrid.Min) + block.CubeGrid.GridIntegerToWorld(block.CubeGrid.Max)) / 2;
}

//------------------------------ Thruster Control Methods ------------------------------

void FireThrusters(List<IMyTerminalBlock> listThrusters, bool overrideMode)
{
if (listThrusters != null)
{
for (int i = 0; i < listThrusters.Count; i++)
{
listThrusters[i].SetValue<float>("Override", (overrideMode ? listThrusters[i].GetMaximum<float>("Override") : 0f));
}
}
}

//------------------------------ Gyroscope Control Methods ------------------------------

void SetGyroOverride(bool bOverride)
{
for (int i = 0; i < gyroscopes.Count; i++)
{
if (((IMyGyro)gyroscopes[i]).GyroOverride != bOverride)
{
gyroscopes[i].ApplyAction("Override");
}
}
}

void SetGyroYaw(double yawRate)
{
for (int i = 0; i < gyroscopes.Count; i++)
{
gyroscopes[i].SetValueFloat(gyroYawField[i], (float)yawRate * gyroYawFactor[i]);
}
}

void SetGyroPitch(double pitchRate)
{
for (int i = 0; i < gyroscopes.Count; i++)
{
gyroscopes[i].SetValueFloat(gyroPitchField[i], (float)pitchRate * gyroPitchFactor[i]);
}
}

void SetGyroRoll(double rollRate)
{
for (int i = 0; i < gyroscopes.Count; i++)
{
gyroscopes[i].SetValueFloat(gyroRollField[i], (float)rollRate * gyroRollFactor[i]);
}
}

void ZeroTurnGyro()
{
for (int i = 0; i < gyroscopes.Count; i++)
{
gyroscopes[i].SetValueFloat(gyroYawField[i], 0);
gyroscopes[i].SetValueFloat(gyroPitchField[i], 0);
}
}

void ResetGyro()
{
for (int i = 0; i < gyroscopes.Count; i++)
{
gyroscopes[i].SetValueFloat("Yaw", 0);
gyroscopes[i].SetValueFloat("Pitch", 0);
gyroscopes[i].SetValueFloat("Roll", 0);
}
}

//------------------------------ Name Finder API ------------------------------

IMyTerminalBlock GetSingleBlockWithName(string name)
{
List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
GridTerminalSystem.SearchBlocksOfName(name, blocks);

return (blocks.Count > 0 ? blocks[0] : null);
}

List<IMyTerminalBlock> GetBlocksOfType<T>() where T: class, IMyTerminalBlock
{
List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
GridTerminalSystem.GetBlocksOfType<T>(blocks);

return blocks;
}

List<T> GetBlocksOfTypeCasted<T>() where T: class, IMyTerminalBlock
{
List<T> blocks = new List<T>();
GridTerminalSystem.GetBlocksOfType<T>(blocks);

return blocks;
}

List<IMyTerminalBlock> GetBlocksWithName(string name)
{
List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
GridTerminalSystem.SearchBlocksOfName(name, blocks);

return blocks;
}

List<IMyTerminalBlock> GetBlocksWithName<T>(string name, int matchType = 0) where T: class, IMyTerminalBlock
{
List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
GridTerminalSystem.SearchBlocksOfName(name, blocks);

List<IMyTerminalBlock> filteredBlocks = new List<IMyTerminalBlock>();
for (int i = 0; i < blocks.Count; i++)
{
if (matchType > 0)
{
bool isMatch = false;

switch (matchType)
{
case 1:
if (blocks[i].CustomName.StartsWith(name, StringComparison.OrdinalIgnoreCase))
{
isMatch = true;
}
break;
case 2:
if (blocks[i].CustomName.EndsWith(name, StringComparison.OrdinalIgnoreCase))
{
isMatch = true;
}
break;
case 3:
if (blocks[i].CustomName.Equals(name, StringComparison.OrdinalIgnoreCase))
{
isMatch = true;
}
break;
default:
isMatch = true;
break;
}

if (!isMatch)
{
continue;
}
}

IMyTerminalBlock block = blocks[i] as T;
if (block != null)
{
filteredBlocks.Add(block);
}
}

return filteredBlocks;
}

public class CustomConfiguration
{
public IMyTerminalBlock configBlock;
public Dictionary<string, string> config;

public CustomConfiguration(IMyTerminalBlock block)
{
configBlock = block;
config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

public void Load()
{
ParseCustomData(configBlock, config);
}

public void Save()
{
WriteCustomData(configBlock, config);
}

public string Get(string key, string defVal = null)
{
return config.GetValueOrDefault(key.Trim(), defVal);
}

public void Get(string key, ref string res)
{
string val;
if (config.TryGetValue(key.Trim(), out val))
{
res = val;
}
}

public void Get(string key, ref int res)
{
int val;
if (int.TryParse(Get(key), out val))
{
res = val;
}
}

public void Get(string key, ref float res)
{
float val;
if (float.TryParse(Get(key), out val))
{
res = val;
}
}

public void Get(string key, ref double res)
{
double val;
if (double.TryParse(Get(key), out val))
{
res = val;
}
}

public void Get(string key, ref bool res)
{
bool val;
if (bool.TryParse(Get(key), out val))
{
res = val;
}
}
public void Get(string key, ref bool? res)
{
bool val;
if (bool.TryParse(Get(key), out val))
{
res = val;
}
}

public void Set(string key, string value)
{
config[key.Trim()] = value;
}

public static void ParseCustomData(IMyTerminalBlock block, Dictionary<string, string> cfg, bool clr = true)
{
if (clr)
{
cfg.Clear();
}

string[] arr = block.CustomData.Split(new char[] {'\r','\n'}, StringSplitOptions.RemoveEmptyEntries);
for (int i = 0; i < arr.Length; i++)
{
string ln = arr[i];
string va;

int p = ln.IndexOf('=');
if (p > -1)
{
va = ln.Substring(p + 1);
ln = ln.Substring(0, p);
}
else
{
va = "";
}
cfg[ln.Trim()] = va.Trim();
}
}

public static void WriteCustomData(IMyTerminalBlock block, Dictionary<string, string> cfg)
{
StringBuilder sb = new StringBuilder(cfg.Count * 100);
foreach (KeyValuePair<string, string> va in cfg)
{
sb.Append(va.Key).Append('=').Append(va.Value).Append('\n');
}
block.CustomData = sb.ToString();
}
}

public class VectorAverageFilter
{
public int vectorIndex = 0;
public Vector3D[] vectorArr = null;
public Vector3D vectorSum = new Vector3D();

public VectorAverageFilter(int size)
{
vectorArr = new Vector3D[size];
}

public void Filter(ref Vector3D vectorIn, out Vector3D vectorOut)
{
vectorSum -= vectorArr[vectorIndex];
vectorArr[vectorIndex] = vectorIn;
vectorSum += vectorArr[vectorIndex];
vectorIndex++;
if (vectorIndex >= vectorArr.Length)
{
vectorIndex = 0;
}
vectorOut = vectorSum / vectorArr.Length;
}

public void Set(ref Vector3D vector)
{
vectorSum = default(Vector3D);
for (int i = 0; i < vectorArr.Length; i++)
{
vectorArr[i] = vector;
vectorSum += vectorArr[i];
}
}
}
    #endregion in-game
}
