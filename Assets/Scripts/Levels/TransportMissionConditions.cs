using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransportMissionConditions : SmoothFlyingMissionConditions
{
    [SerializeField] private Collider EndCollider;

    [SerializeField] private bool AllowDamage;
    [SerializeField] private bool AllowStructureChanges;
    [SerializeField] private bool AllowTouchingWater;

    private bool? TransportMissionResult = true;


    public override void Update()
    {
        base.Update();

        if (!AllowDamage)
            ServiceProvider.Instance.PlayerAircraft.AircraftDamaged += OnAircraftDamaged;

        if (!AllowStructureChanges)
            ServiceProvider.Instance.PlayerAircraft.AircraftStructureChanged += OnAircraftStructureChanged;

        if (!AllowTouchingWater)
            ServiceProvider.Instance.PlayerAircraft.PartEnteredWater += OnPartEnteredWater;
    }

    public override bool? MissionResult
    {
        get
        {
            return TransportMissionResult;
        }
    }

    public override Collider EndMissionCollider
    {
        get
        {
            return EndCollider;
        }
    }

    public override string FailFormatScore => base.FailFormatScore;

    private void OnAircraftDamaged(object sender, EventArgs e)
    {
        TransportMissionResult = false;
    }

    private void OnAircraftStructureChanged(object sender, EventArgs e)
    {
        TransportMissionResult = false;
    }

    private void OnPartEnteredWater(object sender, EventArgs e)
    {
        TransportMissionResult = false;
    }
}
