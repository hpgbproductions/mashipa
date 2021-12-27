// Mission Conditions must derive from this class!

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SmoothFlyingMissionConditions : MonoBehaviour
{
    public virtual bool? MissionResult { get; }
    public virtual string FailFormatScore { get { return "-9999"; } }
    public virtual Collider EndMissionCollider { get; }

    private float LandedTime = 0f;
    private float RequiredLandedTime = 1f;

    // True if the aircraft is landed in the End Mission area after completing the mission
    public bool MissionFullyComplete
    {
        get
        {
            if (MissionResult == true)
            {
                return LandedTime > RequiredLandedTime;
            }
            else
            {
                return false;
            }
        }
    }

    public virtual void Update()
    {
        if (EndMissionCollider.bounds.Contains(ServiceProvider.Instance.PlayerAircraft.MainCockpitPosition) &&
                ServiceProvider.Instance.PlayerAircraft.Velocity.magnitude < 0.5f)
        {
            LandedTime += Time.deltaTime;
        }
        else
        {
            LandedTime = 0f;
        }
    }
}
