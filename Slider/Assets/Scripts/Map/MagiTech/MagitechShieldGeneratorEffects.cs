using System.Collections.Generic;
using UnityEngine;

public class MagitechShieldGeneratorEffects : MonoBehaviour 
{
    public List<GameObject> gameObjects;
    private bool areGameObjectsOn;

    private void Start() 
    {
        SetGameObjectsOn(MagiTechGrid.IsInPast(transform));
    }

    private void Update() 
    {
        if (MagiTechGrid.IsInPast(transform) != areGameObjectsOn)
        {
            SetGameObjectsOn(!areGameObjectsOn);
        }
    }

    private void SetGameObjectsOn(bool value)
    {
        areGameObjectsOn = value;
        foreach (GameObject g in gameObjects)
        {
            g.SetActive(areGameObjectsOn);
        }
    }
}