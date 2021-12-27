// Set the simulated world coordinates and date of the sky component.
// This slightly increases the realism of your custom map.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SkySimulationSettings : MonoBehaviour
{
    // BEGIN values are loaded at the start
    [SerializeField] private float Latitude = 0;
    [SerializeField] private float Longitude = 0;
    [SerializeField] private int TimeZone = 0;

    [SerializeField] private int Day = 15;
    [SerializeField] private int Month = 6;
    [SerializeField] private int Year = 2000;
    // END values are loaded at the start

    private Component SkyDomeComponent;
    private Type SkyDomeType;

    private FieldInfo WorldParamsInfo;
    private Type WorldParamsType;
    private object WorldParamsObj;
    private FieldInfo LatitudeInfo;
    private FieldInfo LongitudeInfo;
    private FieldInfo UTCInfo;

    private FieldInfo CycleParamsInfo;
    private Type CycleParamsType;
    private object CycleParamsObj;
    private FieldInfo DayInfo;
    private FieldInfo MonthInfo;
    private FieldInfo YearInfo;

    private void Awake()
    {
        ServiceProvider.Instance.DevConsole.RegisterCommand<float, float, int>("SkySimCoords", SetCoords);
        ServiceProvider.Instance.DevConsole.RegisterCommand<int, int, int>("SkySimDate", SetDate);
    }

    private void Start()
    {
        Component[] components = FindObjectsOfType<Component>();
        foreach (Component c in components)
        {
            if (c.GetType().Name == "TOD_Sky")
            {
                SkyDomeComponent = c;
                SkyDomeType = c.GetType();
                break;
            }
        }

        WorldParamsInfo = SkyDomeType.GetField("World");
        WorldParamsType = WorldParamsInfo.FieldType;
        WorldParamsObj = WorldParamsInfo.GetValue(SkyDomeComponent);
        LatitudeInfo = WorldParamsType.GetField("Latitude");
        LongitudeInfo = WorldParamsType.GetField("Longitude");
        UTCInfo = WorldParamsType.GetField("UTC");

        CycleParamsInfo = SkyDomeType.GetField("Cycle");
        CycleParamsType = CycleParamsInfo.FieldType;
        CycleParamsObj = CycleParamsInfo.GetValue(SkyDomeComponent);
        DayInfo = CycleParamsType.GetField("Day");
        MonthInfo = CycleParamsType.GetField("Month");
        YearInfo = CycleParamsType.GetField("Year");

        SetCoords(Latitude, Longitude, TimeZone);
        SetDate(Day, Month, Year);
    }

    private void SetCoords(float lat, float lon, int utc)
    {
        LatitudeInfo.SetValue(WorldParamsObj, lat);
        LongitudeInfo.SetValue(WorldParamsObj, lon);
        UTCInfo.SetValue(WorldParamsObj, utc);
    }

    private void SetDate(int d, int m, int y)
    {
        DayInfo.SetValue(CycleParamsObj, d);
        MonthInfo.SetValue(CycleParamsObj, m);
        YearInfo.SetValue(CycleParamsObj, y);
    }

    private void OnDestroy()
    {
        ServiceProvider.Instance.DevConsole.UnregisterCommand("SkySimCoords");
        ServiceProvider.Instance.DevConsole.UnregisterCommand("SkySimDate");
    }
}
