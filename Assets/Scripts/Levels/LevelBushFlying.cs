using System.Collections;
using System.Collections.Generic;
using Jundroo.SimplePlanes.ModTools;
using UnityEngine;

public class LevelBushFlying : SmoothFlyingLevelBase
{
    private static readonly string Name = "FPK - Cross Country";

    private static readonly string LevelDescription =
        @"Cross Country
Time: 0800
Cloud Cover: Few Clouds
Wind Direction: Random
Wind Speed: 20-30 kts

Deliver packs of a white mixture (let's call them ""eggs"", because it will get messy if they break) between airfields.

Make sure your aircraft is kitted out for the strong winds and bumpy runways you will be facing.

[ Cargo Conditions ]
900 lbs (~400 kg)
<color=yellow>Cargo is to be added to your aircraft as dead weight. The check is performed through file access.</color>

[ Time Conditions ]
Time limit of 25 minutes, after which 100 points are deducted every second.

[ Smoothness Conditions ]
VerticalG (+0.2 to +2.0) x1000
PitchAngle (-20 to +20) x1000
RollAngle (-30 to +30) x1000";

    private static readonly string LevelGameObject = "LevelBushFlying";

    public LevelBushFlying()
        : base(Name, LevelDescription, LevelGameObject)
    {
    }

    protected override LevelStartLocation StartLocation
    {
        get
        {
            return StartLocations.WW2Base;
        }
    }

    protected override float TimeOfDay
    {
        get
        {
            return 8f;
        }
    }

    protected override WeatherPreset Weather
    {
        get
        {
            return WeatherPreset.FewClouds;
        }
    }

    protected override string CriticalDamageMessage => base.CriticalDamageMessage;

    protected override string StartMessage
    {
        get
        {
            return "Head to the East Bay Airstrip.";
        }
    }

    protected override string NotEnoughCargoMessage => base.NotEnoughCargoMessage;

    protected override string SmoothnessFailureMessage
    {
        get
        {
            return "You broke the eggs.";
        }
    }

    protected override string MissionFailureMessage
    {
        get
        {
            return "You broke the eggs.";
        }
    }

    protected override string SuccessLateMessage
    {
        get
        {
            return "It could be faster.";
        }
    }

    protected override string SuccessMessage
    {
        get
        {
            return "The package made it in one piece!";
        }
    }
}
