// Provides methods for modifying the wind through reflection.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class WindController : MonoBehaviour
{
    private Component WindManager;    // WindManager
    private Type WindManagerType;

    private PropertyInfo WindHeadingInfo;       // int WindHeading / degrees blowing towards
    private PropertyInfo WindSpeedInfo;         // int WindSpeed / mph magnitude
    private PropertyInfo WindVelocityInfo;      // Vector3 WindVelocity / m/s global
    private PropertyInfo WindGustModeInfo;      // WindGustMode GustMode
    private MethodInfo StringToGustModeInfo;    // WindGustMode WindGustModeFromText(string)

    // Automatically set the wind parameters
    [SerializeField] private int MinHeading;
    [SerializeField] private int MaxHeading;
    [SerializeField] private int MinSpeedMph;
    [SerializeField] private int MaxSpeedMph;
    [SerializeField] private WindGustMode GustMode;
    private int WindHeading;
    private int WindSpeedMph;

    private void Awake()
    {
        Component[] allComponents = FindObjectsOfType<Component>();
        foreach (Component c in allComponents)
        {
            Type cType = c.GetType();
            if (cType.Name == "WindManager")
            {
                WindManager = c;
                WindManagerType = cType;
                break;
            }
        }

        WindHeadingInfo = WindManagerType.GetProperty("WindHeading");
        WindSpeedInfo = WindManagerType.GetProperty("WindSpeed");
        WindVelocityInfo = WindManagerType.GetProperty("WindVelocity");
        WindGustModeInfo = WindManagerType.GetProperty("GustMode");
        StringToGustModeInfo = WindManagerType.GetMethod("WindGustModeFromText");

        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);
    }

    private void Start()
    {
        // Apply random wind parameters
        WindHeading = UnityEngine.Random.Range(MinHeading, MaxHeading + 1);
        WindSpeedMph = UnityEngine.Random.Range(MinSpeedMph, MaxSpeedMph + 1);
        SetWind(WindHeading, WindSpeedMph);
        SetGustMode(GustMode);
    }

    private void SetWind(int heading, int speed_mph)
    {
        WindHeadingInfo.SetValue(WindManager, heading);
        WindSpeedInfo.SetValue(WindManager, speed_mph);
    }

    private void SetGustMode(WindGustMode mode)
    {
        var gustMode = StringToGustModeInfo.Invoke(WindManager, new object[] { mode.ToString() });

        WindGustModeInfo.SetValue(WindManager, gustMode);
    }

    public Vector3 GetWind()
    {
        return (Vector3)WindVelocityInfo.GetValue(WindManager);
    }

    private enum WindGustMode
    {
        None, Light, Medium, Heavy
    }
}
