using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

public class SmoothFlyingMonitor : MonoBehaviour
{
    private Transform PlayerTransform;

    [SerializeField] private float StartScore = 9999.9f;
    private float CurrentScore;
    [NonSerialized] public int TotalScore;

    [SerializeField] private float TimeLimit;
    [SerializeField] private float TimePenaltyRate;
    [NonSerialized] public float TimePenalty;

    [SerializeField] private int TestMode = 0;

    private int FrameCounter = 0;    // Used for aircraft mass check and Test Mode debug text

    // BEGIN aircraft mass check
    // * Dead weight is checked on the 100th FixedUpdate

    public float MinimumCargoMass;
    [NonSerialized] public float AircraftCargoMass = 0f;

    private string AircraftFolderName = "AircraftDesigns";
    private string AircraftEditorName = "__editor__.xml";
    private string AircraftEditorPath;

    private string CheckFuselagePattern = @"<Fuselage\.State.*?deadWeight=""([0-9.-]*?)"".*?\/>";
    public bool? CheckFuselage = null;

    // END aircraft mass check

    // BEGIN aircraft reflection

    private Type AircraftScriptType;
    private Component AircraftScript;
    private PropertyInfo PitchRateInfo;    // float PitchRate
    private PropertyInfo YawRateInfo;      // float YawRate
    private PropertyInfo RollRateInfo;     // float RollRate

    private PropertyInfo VerticalGInfo;        // float VerticalG
    private PropertyInfo GForceInfo;           // float GForce
    private PropertyInfo IASInfo;              // float IAS
    private PropertyInfo TASInfo;              // float TAS
    private PropertyInfo AngleOfAttackInfo;    // float AngleOfAttack
    private PropertyInfo AngleOfSlipInfo;      // float AngleOfSlip

    // END aircraft reflection

    // BEGIN flight telemetry

    private Vector3 WorldPositionCurrent;    // World space position

    private Vector3 WorldVelocityCurrent;    // World space velocity

    private Vector3 SelfVelocityCurrent;     // Relative velocity
    private Vector3 SelfVelocityPrevious;

    private Vector3 SelfAccelerationCurrent;     // Relative acceleration, including gravititational, felt by the cargo or passegers

    private Vector3 WorldRotationCurrent;     // PitchAngle, Heading, RollAngle

    private Vector3 SelfAngularVelocityCurrent;
    private Vector3 SelfAngularVelocityPrevious;

    private Vector3 SelfAngularAccelerationCurrent;

    // END flight telemetry

    // GUI
    [SerializeField] private GUISkin Skin;
    private GUIStyle ScoreStyle;
    private GUIStyle TimeStyle;
    private GUIStyle BgStyle;

    public SmoothnessCondition[] FlightSmoothnessConditions;

    private void Awake()
    {
        CurrentScore = StartScore;

        GameObject playerObject = new GameObject("PlayerTransform");
        PlayerTransform = playerObject.transform;
        PlayerTransform.parent = gameObject.transform;

        List<GameObject> parts = ServiceProvider.Instance.PlayerAircraft.Parts;
        foreach (GameObject part in parts)
        {
            if (AircraftScript == null)
            {
                Component[] parentComponents = part.GetComponentsInParent<Component>();
                foreach (Component c in parentComponents)
                {
                    Type cType = c.GetType();
                    if (cType.Name == "AircraftScript")
                    {
                        AircraftScript = c;
                        AircraftScriptType = cType;
                        break;
                    }
                }
            }
        }

        PitchRateInfo = AircraftScriptType.GetProperty("PitchRate", BindingFlags.NonPublic | BindingFlags.Instance);
        YawRateInfo = AircraftScriptType.GetProperty("YawRate", BindingFlags.NonPublic | BindingFlags.Instance);
        RollRateInfo = AircraftScriptType.GetProperty("RollRate", BindingFlags.NonPublic | BindingFlags.Instance);

        VerticalGInfo = AircraftScriptType.GetProperty("VerticalG", BindingFlags.NonPublic | BindingFlags.Instance);
        GForceInfo = AircraftScriptType.GetProperty("GForce", BindingFlags.NonPublic | BindingFlags.Instance);
        IASInfo = AircraftScriptType.GetProperty("IAS", BindingFlags.NonPublic | BindingFlags.Instance);
        TASInfo = AircraftScriptType.GetProperty("TAS", BindingFlags.NonPublic | BindingFlags.Instance);
        AngleOfAttackInfo = AircraftScriptType.GetProperty("AngleOfAttack", BindingFlags.NonPublic | BindingFlags.Instance);
        AngleOfSlipInfo = AircraftScriptType.GetProperty("AngleOfSlip", BindingFlags.NonPublic | BindingFlags.Instance);

        BgStyle = Skin.box;
        ScoreStyle = Skin.label;
        TimeStyle = Skin.customStyles[0];
    }

    private void FixedUpdate()
    {
        if (ServiceProvider.Instance.GameState.IsPaused)
        {
            // Prevents NaN values from being written
            return;
        }

        // Update player aircraft tracker
        PlayerTransform.position = ServiceProvider.Instance.PlayerAircraft.MainCockpitPosition;
        PlayerTransform.eulerAngles = ServiceProvider.Instance.PlayerAircraft.MainCockpitRotation;

        // Update Current values
        WorldPositionCurrent = ServiceProvider.Instance.PlayerAircraft.MainCockpitPosition + ServiceProvider.Instance.GameWorld.FloatingOriginOffset;
        WorldVelocityCurrent = ServiceProvider.Instance.PlayerAircraft.Velocity;
        SelfVelocityCurrent = PlayerTransform.InverseTransformVector(WorldVelocityCurrent);
        SelfAccelerationCurrent = (SelfVelocityCurrent - SelfVelocityPrevious) / Time.fixedDeltaTime + PlayerTransform.InverseTransformVector(Physics.gravity);
        WorldRotationCurrent = new Vector3(
            Mathf.DeltaAngle(0f, ServiceProvider.Instance.PlayerAircraft.MainCockpitRotation.x),
            Mathf.DeltaAngle(0f, ServiceProvider.Instance.PlayerAircraft.MainCockpitRotation.y),
            Mathf.DeltaAngle(0f, ServiceProvider.Instance.PlayerAircraft.MainCockpitRotation.z));
        SelfAngularVelocityCurrent = new Vector3(
            (float)PitchRateInfo.GetValue(AircraftScript),
            (float)YawRateInfo.GetValue(AircraftScript),
            (float)RollRateInfo.GetValue(AircraftScript));
        SelfAngularAccelerationCurrent = new Vector3(Mathf.DeltaAngle(SelfAngularVelocityPrevious.x, SelfAngularVelocityCurrent.x),
            Mathf.DeltaAngle(SelfAngularVelocityPrevious.y, SelfAngularVelocityCurrent.y),
            Mathf.DeltaAngle(SelfAngularVelocityPrevious.z, SelfAngularVelocityCurrent.z)) / Time.fixedDeltaTime;

        // Check flight performance
        // Only start checking after a while to prevent losses from spawning
        if (Time.timeSinceLevelLoad > 3f)
        {
            foreach (SmoothnessCondition sc in FlightSmoothnessConditions)
            {
                CurrentScore -= sc.GetScoreDeduction(GetSmoothnessConditionValue(sc), Time.fixedDeltaTime);
            }
            TimePenalty = Mathf.Max(0f, (Time.timeSinceLevelLoad - TimeLimit) * TimePenaltyRate);
            TotalScore = Mathf.FloorToInt(CurrentScore - TimePenalty);
        }

        FrameCounter++;
        if (FrameCounter == 300)
        {
            CheckFuselageDeadWeight();
        }
        if (FrameCounter % 10 == 0)
        {
            if (TestMode == 1)
            {
                ServiceProvider.Instance.GameWorld.ShowStatusMessage(string.Format("{0} | {1}\n{2} | {3} | {4}\n{5}/{6}\nScore: {7}",
                    SelfVelocityCurrent, SelfAccelerationCurrent,
                    WorldRotationCurrent, SelfAngularVelocityCurrent, SelfAngularAccelerationCurrent,
                    AircraftCargoMass, MinimumCargoMass, CurrentScore));
            }
            else if (TestMode == 2)
            {
                string DisplayString = string.Empty;
                for (int i = 0; i < FlightSmoothnessConditions.Length; i++)
                {
                    DisplayString += string.Format("Condition {0}: v={1}; d={2}\n",
                        i, FlightSmoothnessConditions[i].CurrentValue, FlightSmoothnessConditions[i].CurrentDeduction);
                }
                ServiceProvider.Instance.GameWorld.ShowStatusMessage(string.Format("{0} {1}/{2} | {3}",
                    DisplayString, AircraftCargoMass, MinimumCargoMass, TotalScore));
            }
        }

        // Update Previous values
        SelfVelocityPrevious = SelfVelocityCurrent;
        SelfAngularVelocityPrevious = SelfAngularVelocityCurrent;
    }

    private void CheckFuselageDeadWeight()
    {
        AircraftEditorPath = Path.Combine(Application.persistentDataPath, AircraftFolderName, AircraftEditorName);
        string editorData = File.ReadAllText(AircraftEditorPath);
        foreach (Match match in Regex.Matches(editorData, CheckFuselagePattern))
        {
            AircraftCargoMass += float.Parse(match.Groups[1].Value);
        }
        CheckFuselage = AircraftCargoMass > MinimumCargoMass;
        Debug.Log($"Cargo detected: {AircraftCargoMass} kg (required {MinimumCargoMass} kg)");
    }

    private float GetSmoothnessConditionValue(SmoothnessCondition sc)
    {
        Vector3 conditionVector = Vector3.zero;
        switch (sc.ConditionType)
        {
            case SmoothnessConditions.VerticalG:
                return (float)VerticalGInfo.GetValue(AircraftScript);
            case SmoothnessConditions.GForce:
                return (float)GForceInfo.GetValue(AircraftScript);
            case SmoothnessConditions.IAS:
                return (float)IASInfo.GetValue(AircraftScript);
            case SmoothnessConditions.TAS:
                return (float)TASInfo.GetValue(AircraftScript);
            case SmoothnessConditions.AngleOfAttack:
                return (float)AngleOfAttackInfo.GetValue(AircraftScript);
            case SmoothnessConditions.AngleOfSlip:
                return (float)AngleOfSlipInfo.GetValue(AircraftScript);
            case SmoothnessConditions.WorldPosition:
                conditionVector = WorldPositionCurrent;
                break;
            case SmoothnessConditions.WorldVelocity:
                conditionVector = WorldVelocityCurrent;
                break;
            case SmoothnessConditions.SelfVelocity:
                conditionVector = SelfVelocityCurrent;
                break;
            case SmoothnessConditions.SelfAcceleration:
                conditionVector = SelfAccelerationCurrent;
                break;
            case SmoothnessConditions.WorldRotation:
                conditionVector = WorldRotationCurrent;
                break;
            case SmoothnessConditions.SelfAngularVelocity:
                conditionVector = SelfAngularVelocityCurrent;
                break;
            case SmoothnessConditions.SelfAngularAcceleration:
                conditionVector = SelfAngularAccelerationCurrent;
                break;
            default:
                Debug.LogError("GetSmoothnessConditionValue: Invalid value of ConditionType!");
                break;
        }

        switch (sc.ConditionComponent)
        {
            case VectorComponents.x:
                return conditionVector.x;
            case VectorComponents.y:
                return conditionVector.y;
            case VectorComponents.z:
                return conditionVector.z;
            case VectorComponents.horizontal:
                return Mathf.Sqrt(conditionVector.x * conditionVector.x + conditionVector.z * conditionVector.z);
            case VectorComponents.magnitude:
                return conditionVector.magnitude;
            default:
                Debug.LogError("GetSmoothnessConditionValue: Invalid value of ConditionComponent!");
                return 0f;
        }
    }

    [Serializable]
    public class SmoothnessCondition
    {
        // The type of value to monitor
        public SmoothnessConditions ConditionType;
        public VectorComponents ConditionComponent;

        public float SafeUpperBound;
        public float DangerUpperBound;
        public float SafeLowerBound;
        public float DangerLowerBound;

        public float DeductionMultiplier;    // Maximum deduction in points/second
        public float DeductionAttack;        // Rate of increase
        public float DeductionDecay;         // Rate of decrease

        [NonSerialized] public float CurrentDeduction = 0f;    // Percentage of deduction between 0 and 1
        [NonSerialized] public float CurrentValue;

        public float GetScoreDeduction(float value, float deltaTime)
        {
            float TargetDeduction = 0;
            CurrentValue = value;
            if (value > SafeUpperBound)
            {
                TargetDeduction = Mathf.InverseLerp(SafeUpperBound, DangerUpperBound, value);
            }
            else if (value < SafeLowerBound)
            {
                TargetDeduction = Mathf.InverseLerp(SafeLowerBound, DangerLowerBound, value);
            }

            if (TargetDeduction > CurrentDeduction)
            {
                CurrentDeduction = Mathf.Min(CurrentDeduction + DeductionAttack * deltaTime, TargetDeduction);
            }
            else if (TargetDeduction < CurrentDeduction)
            {
                CurrentDeduction = Mathf.Max(CurrentDeduction - DeductionDecay * deltaTime, TargetDeduction);
            }

            return CurrentDeduction * DeductionMultiplier * deltaTime;
        }
    }

    public enum SmoothnessConditions
    {
        WorldPosition, WorldVelocity, SelfVelocity, SelfAcceleration, WorldRotation, SelfAngularVelocity, SelfAngularAcceleration,
        VerticalG, GForce, IAS, TAS, AngleOfAttack, AngleOfSlip
    }

    public enum VectorComponents
    {
        x, y, z, horizontal, magnitude
    }

    private void OnGUI()
    {
        int RemainingTime = Mathf.Max(Mathf.RoundToInt(TimeLimit - Time.timeSinceLevelLoad), 0);

        GUI.Box(new Rect(Screen.width - 250, 0, 250, 100), "", BgStyle);
        GUI.Label(new Rect(Screen.width - 220, 15, 200, 50), string.Format("{0}", TotalScore), ScoreStyle);
        GUI.Label(new Rect(Screen.width - 220, 60, 200, 30), string.Format("TIME: {0:D2}' {1:D2}\"", RemainingTime / 60, RemainingTime % 60), TimeStyle);
    }
}
