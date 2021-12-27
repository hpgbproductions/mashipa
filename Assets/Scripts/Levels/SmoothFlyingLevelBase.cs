using Jundroo.SimplePlanes.ModTools;
using System;
using UnityEngine;

/// <summary>
/// A SimplePlanes custom level.
/// </summary>
public abstract class SmoothFlyingLevelBase : Level
{
    private string _levelGameObjectName;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmoothFlyingLevelBase"/> class.
    /// </summary>
    public SmoothFlyingLevelBase(string levelName, string levelDescription, string levelGameObjectName)
        : base(levelName, "Flight Park Kirama", levelDescription)
    {
        this._levelGameObjectName = levelGameObjectName;
    }

    protected SmoothFlyingMonitor SmoothFlyingMonitor { get; set; }
    protected SmoothFlyingMissionConditions MissionConditions { get; set; }

    protected bool? Initialized { get; set; }

    protected virtual float TimeOfDay
    {
        get
        {
            return 12f;
        }
    }

    protected virtual WeatherPreset Weather
    {
        get
        {
            return WeatherPreset.FewClouds;
        }
    }

    protected virtual string CriticalDamageMessage
    {
        get
        {
            return "Your aircraft has been critically damaged.";
        }
    }

    protected virtual string StartMessage
    {
        get
        {
            return null;
        }
    }

    protected virtual string NotEnoughCargoMessage
    {
        get
        {
            return "You did not carry the required amount of cargo.";
        }
    }

    protected virtual string SmoothnessFailureMessage
    {
        get
        {
            return "The flight was not smooth enough.";
        }
    }

    protected virtual string MissionFailureMessage
    {
        get
        {
            return "Mission failed.";
        }
    }

    protected virtual string SuccessLateMessage
    {
        get
        {
            return "You're late, but you still flew well.";
        }
    }

    protected virtual string SuccessMessage
    {
        get
        {
            return "A successful flight!";
        }
    }

    protected virtual void Initialize()
    {
        if (!Initialized.HasValue)
        {
            try
            {
                GameObject obj = ServiceProvider.Instance.ResourceLoader.LoadGameObject(_levelGameObjectName);

                SmoothFlyingMonitor = obj.GetComponent<SmoothFlyingMonitor>();
                if (SmoothFlyingMonitor == null)
                {
                    Debug.LogError("Unable to get SmoothFlyingMonitor!");
                    Initialized = false;
                    return;
                }

                MissionConditions = obj.GetComponent<SmoothFlyingMissionConditions>();
                if (MissionConditions == null)
                {
                    Debug.LogError("Unable to get SmoothFlyingMissionConditions!");
                    Initialized = false;
                    return;
                }

                if (!string.IsNullOrEmpty(StartMessage))
                {
                    ServiceProvider.Instance.GameWorld.ShowStatusMessage(StartMessage);
                }

                ServiceProvider.Instance.EnvironmentManager.UpdateTimeOfDay(TimeOfDay, 0f, true, true);
                ServiceProvider.Instance.EnvironmentManager.UpdateWeather(Weather, 0f, true);

                Initialized = true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                Initialized = false;
            }
        }
    }

    protected override string FormatScore(float score)
    {
        if (SmoothFlyingMonitor.CheckFuselage == false)
        {
            return string.Format("{0:F1}/{1} kg", SmoothFlyingMonitor.AircraftCargoMass, SmoothFlyingMonitor.MinimumCargoMass);
        }
        else if (MissionConditions.MissionResult == false || ServiceProvider.Instance.PlayerAircraft.CriticallyDamaged)
        {
            return MissionConditions.FailFormatScore;
        }
        else
        {
            return score.ToString();
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!(Initialized ?? false))
        {
            Initialize();
            return;
        }

        if (SmoothFlyingMonitor.CheckFuselage == false)
        {
            EndLevel(false, NotEnoughCargoMessage, -9999);
        }
        else if (MissionConditions.MissionFullyComplete)
        {
            if (SmoothFlyingMonitor.TotalScore < 0)
                EndLevel(false, SmoothnessFailureMessage, SmoothFlyingMonitor.TotalScore);
            else if (SmoothFlyingMonitor.TimePenalty < 0.1f)
                EndLevel(true, SuccessMessage, SmoothFlyingMonitor.TotalScore);
            else
                EndLevel(true, SuccessLateMessage, SmoothFlyingMonitor.TotalScore);
        }
        else if (ServiceProvider.Instance.PlayerAircraft.CriticallyDamaged)
        {
            EndLevel(false, CriticalDamageMessage, -9999);
        }
        else if (MissionConditions.MissionResult == false)
        {
            EndLevel(false, MissionFailureMessage, -9999);
        }
    }
}