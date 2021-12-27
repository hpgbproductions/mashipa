using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jundroo.SimplePlanes.ModTools;
using UnityEngine;

/// <summary>
/// A collection of common starting locations.
/// </summary>
public static class StartLocations
{
    public static LevelStartLocation South
    {
        get
        {
            return new LevelStartLocation()
            {
                Position = new Vector3(-20599f, 5f, -39996f),
                Rotation = new Vector3(0f, 20f, 0f),
                InitialSpeed = 0f,
                InitialThrottle = 0f,
                StartOnGround = true,
            };
        }
    }

    public static LevelStartLocation WW2Base
    {
        get
        {
            return new LevelStartLocation()
            {
                Position = new Vector3(-11466f, 32f, -7717f),
                Rotation = new Vector3(0f, 142f, 0f),
                InitialSpeed = 0f,
                InitialThrottle = 0f,
                StartOnGround = true,
            };
        }
    }
}