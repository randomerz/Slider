using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveLight : MonoBehaviour
{
    public bool lightOn;

    [Header("Lighting Options")]
    public Vector2Int lightDir;
    public float lightRadius;
    public float lightArcAngle;

    [Header("Grid Update")]
    public CaveGrid grid;

    public CaveSTile stile;

    public enum LightType
    {
        UniDirectional,
        AllDirections,
    }
    public LightType type;

    public class OnLightDirChangedArgs
    {
        public Vector2Int lightDir;
    }

    // Start is called before the first frame update
    void Start()
    {
        lightOn = true;
        lightDir = new Vector2Int(-1, 0);
    }

    public void UpdateLightMap(bool value)
    {
        grid.SetLit(stile.x, stile.y, value);
        switch (type)
        {
            case LightType.UniDirectional:
                grid.SetLit(stile.x + lightDir.x, stile.y + lightDir.y, value);
                break;
            case LightType.AllDirections:
                grid.SetLit(stile.x + 1, stile.y, value);
                grid.SetLit(stile.x, stile.y + 1, value);
                grid.SetLit(stile.x - 1, stile.y, value);
                grid.SetLit(stile.x, stile.y - 1, value);
                break;
        }
    }

    public void UpdateLightMap(int x, int y, bool value)
    {
        grid.SetLit(x, y, value);
        switch (type)
        {
            case LightType.UniDirectional:
                grid.SetLit(x + lightDir.x, y + lightDir.y, value);
                break;
            case LightType.AllDirections:
                grid.SetLit(x + 1, y, value);
                grid.SetLit(x, y + 1, value);
                grid.SetLit(x - 1, y, value);
                grid.SetLit(x, y - 1, value);
                break;
        }
    }

    //L: Below is for the player to interact with the light, but it's kinda useless since we're not doing that anymore
    public void SetLightDir(Vector2Int value)
    {
        UpdateLightMap(false);
        lightDir = value;
        UpdateLightMap(true);
    }

    public void RotateLight(bool ccw)
    {
        UpdateLightMap(false);
        lightDir = ccw ? new Vector2Int(-lightDir.y, lightDir.x) : new Vector2Int(lightDir.y, -lightDir.x);
        UpdateLightMap(true);
    }

    public void SetLightOn(bool value)
    {
        lightOn = value;
        UpdateLightMap(value);
    }
}
