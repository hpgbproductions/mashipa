using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTrail : MonoBehaviour
{
    [SerializeField] private TrailRenderer effectTrail;
    [SerializeField] private Light effectLight;
    private Transform effectTransform;

    [SerializeField] private float effectVelocity;

    private bool animationActive = false;
    private bool hasTriggered = false;

    private void Update()
    {
        if (animationActive)
        {
            // Tracks the player aircraft to maintain proper effect at all times
            transform.position = ServiceProvider.Instance.PlayerAircraft.MainCockpitPosition;

            effectTransform.Translate(new Vector3(0f, 0f, effectVelocity) * Time.deltaTime, Space.Self);

            if (effectTransform.localPosition.z > 100000)
            {
                EndAnimation();
            }
        }
        else if (!hasTriggered)
        {
            // Perform check for trigger
            if (System.DateTime.Now.Hour == 3 &&
                System.DateTime.Now.Minute == 12 &&
                Mathf.FloorToInt(ServiceProvider.Instance.EnvironmentManager.TimeOfDay) == 3)
            {
                StartAnimation();
            }
        }
    }

    private void StartAnimation()
    {
        animationActive = true;
        hasTriggered = true;

        // Sets a random rotation
        Random.InitState(System.DateTime.Now.Second * 1000 + System.DateTime.Now.Millisecond);
        transform.eulerAngles = new Vector3(0f, Random.Range(0f, 360f), Random.Range(-30f, 30f));

        effectTransform = effectTrail.transform;
        effectTransform.localPosition = new Vector3(0f, 10000f, -100000f);
        effectTrail.enabled = true;
        effectLight.enabled = true;

        ServiceProvider.Instance.GameWorld.ShowStatusMessage("Just like the shooting star I have become...");
    }

    private void EndAnimation()
    {
        animationActive = false;
        effectTrail.enabled = false;
        effectLight.enabled = false;
    }
}
