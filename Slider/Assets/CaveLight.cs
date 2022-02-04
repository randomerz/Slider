using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveLight : MonoBehaviour
{
    public bool lightOn;
    private Vector2Int lightDir;

    public CaveGrid grid;

    public CaveSTile stile;

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

    private void UpdateLightMap(bool value)
    {
        grid.SetLit(stile.x + lightDir.x, stile.y + lightDir.y, value);
    }

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
