using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public CaveLight[] lights;

    public GameObject tilesRoot;

    public List<Material> caveLightMaterials;

    void Start()
    {
        Renderer[] renderers = tilesRoot.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (r.material.GetTag("UsesCaveLights", false, "No").CompareTo("Yes") == 0)
            {
                caveLightMaterials.Add(r.material);
            }
        }
    }

}
