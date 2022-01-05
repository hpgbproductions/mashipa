using System.Collections;
using System.Collections.Generic;
using Jundroo.SimplePlanes.ModTools;
using UnityEngine;

public class LevelRegional : SmoothFlyingLevelBase
{
    private static readonly string Name = "FPK - Regional Airline";

    private static readonly string LevelDescription =
        @"Regional Airline
Time: 1200
Cloud Cover: Few Clouds
Wind Direction: Random
Wind Speed: 10-20 kts

Conduct a routine commuter flight from the South Airport to the North Airport. Just fly north-northeast to reach the location, and in the green area to complete the flight.

Live human passengers are rather picky when it comes to flight smoothness, so be careful and plan your moves ahead of time.

[ Cargo Conditions ]
4,000 lbs (~1,800 kg)
<color=yellow>Cargo is to be added to your aircraft as dead weight. The check is performed through file access.</color>

[ Time Conditions ]
Time limit of 15 minutes, after which 100 points are deducted every second.

[ Smoothness Conditions ]
VerticalG (+0.8 to +1.4) x2000
PitchAngle (-20 to +10) x1000
RollAngle (-30 to +30) x1000";

    private static readonly string LevelGameObject = "KiramaLevelRegional";

    public LevelRegional()
        : base(Name, LevelDescription, LevelGameObject)
    {
    }

    protected override LevelStartLocation StartLocation
    {
        get
        {
            return StartLocations.South;
        }
    }

    protected override float TimeOfDay
    {
        get
        {
            return 12f;
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
            return "Head to the North Airport.";
        }
    }

    protected override string NotEnoughCargoMessage => base.NotEnoughCargoMessage;

    protected override string SmoothnessFailureMessage
    {
        get
        {
            return "The passengers aren't happy with that.";
        }
    }

    protected override string MissionFailureMessage
    {
        get
        {
            return "The passengers aren't happy with that.";
        }
    }

    protected override string SuccessLateMessage
    {
        get
        {
            return "It was smooth enough, but you're late.";
        }
    }

    protected override string SuccessMessage
    {
        get
        {
            return "A great flight!";
        }
    }
}
