// Copy some settings from a parent terrain to its children.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyTerrainSettings : MonoBehaviour
{
    private Terrain ParentTerrain;
    private TerrainCollider ParentCollider;

    private Terrain[] ChildTerrains;
    private TerrainCollider[] ChildColliders;

    private void Start()
    {
        ParentTerrain = GetComponent<Terrain>();
        ParentCollider = GetComponent<TerrainCollider>();

        ChildTerrains = GetComponentsInChildren<Terrain>();
        ChildColliders = GetComponentsInChildren<TerrainCollider>();

        float pixelError = ParentTerrain.heightmapPixelError;
        float baseMapDistance = ParentTerrain.basemapDistance;
        foreach (Terrain t in ChildTerrains)
        {
            t.heightmapPixelError = pixelError;
            t.basemapDistance = baseMapDistance;
        }

        PhysicMaterial physicMaterial = ParentCollider.material;
        foreach (TerrainCollider c in ChildColliders)
        {
            c.material = physicMaterial;
        }
    }
}
