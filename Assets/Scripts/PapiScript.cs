// Place the script and four lights next to the runway.
// There must be four lights (use spot lights with halo).
// Light 0 is closest to the runway and Light 3 is furthest.

// The Z-axis of the script GameObject must face the landing roll direction
// i.e. the aircraft approaches it from the back.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PapiScript : MonoBehaviour
{
    [SerializeField] private Light[] Lights;

    private Vector3 RelPosToAircraft;

    private float glideSlopeAngle;
    private int numRedLights;

    private float relHeadingAngle;
    private float fullLightScale;
    private float lightScale;

    private void Start()
    {
        if (Lights.Length != 4)
        {
            Debug.LogError("The number of lights assigned to the PAPI script must be four!");
        }

        fullLightScale = Lights[0].range;
    }

    private void Update()
    {
        RelPosToAircraft = transform.InverseTransformVector(ServiceProvider.Instance.PlayerAircraft.MainCockpitPosition - transform.position);

        // Set the color according to the glide angle,
        // and the scale according to the aircraft position

        glideSlopeAngle = Mathf.Atan(RelPosToAircraft.y / -RelPosToAircraft.z) * Mathf.Rad2Deg;
        numRedLights = 4 - Mathf.RoundToInt((glideSlopeAngle - 2.33333f) * 3f);

        relHeadingAngle = Mathf.Abs(Mathf.DeltaAngle(Mathf.Atan2(RelPosToAircraft.x, RelPosToAircraft.z) * Mathf.Rad2Deg, 180f));
        lightScale = fullLightScale * Mathf.Clamp01(Mathf.InverseLerp(80f, 60f, relHeadingAngle));

        for (int i = 0; i < Lights.Length; i++)
        {
            if (i < numRedLights) Lights[i].color = Color.red;
            else Lights[i].color = Color.white;

            Lights[i].range = lightScale;
        }

        // Debug
        // ServiceProvider.Instance.GameWorld.ShowStatusMessage(string.Format("{0}\n{1} | {2}\n{3} | {4}",
        //     RelPosToAircraft, glideSlopeAngle, numRedLights, relHeadingAngle, lightScale));
    }
}
