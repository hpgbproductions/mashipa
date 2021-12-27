// Show the sea level as a semi-transparent blue plane.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaLevelGizmo : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 0f, 1f, 0.5f);
        Gizmos.DrawCube(Vector3.zero, new Vector3(500000f, 0.01f, 500000f));
    }
}
