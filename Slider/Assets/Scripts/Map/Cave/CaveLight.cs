using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CaveLight : MonoBehaviour
{
    public bool lightOn;


    [Header("Lighting Options")]
    public Vector2Int lightDir;
    //public float lightRadius;
    //public float lightArcAngle;

    public enum LightType
    {
        UniDirectional,
        AllDirections,
    }
    public LightType type;

    // Start is called before the first frame update
    void Start()
    {
        lightOn = true;
        lightDir = new Vector2Int(-1, 0);
    }

    public Texture2D GetLightMask(int maskOffsetX, int maskOffsetY, int maskSizeX, int maskSizeY)
    {

        Texture2D mask = new Texture2D(maskSizeX, maskSizeY);
        for (int x = 0; x < maskSizeX; x++)
        {
            for (int y = 0; y < maskSizeY; y++)
            {
                mask.SetPixel(x, y, Color.black);
            }
        }
        Vector2Int pos = new Vector2Int((int)transform.position.x, (int)transform.position.y);

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int dir in dirs)
        {
            for (int i = -8; i <= 8; i++)
            {
                Vector2Int start = pos + new Vector2Int(dir.y, dir.x) * i;
                RaycastHit2D hit = Physics2D.Raycast(start, dir, 60);

                int dist = (int) (hit.point - start).magnitude;
                Vector2Int curr = start;
                for (int j=0; j<dist; j++)
                {
                    mask.SetPixel(curr.x - maskOffsetX, curr.y - maskOffsetY, Color.white);
                    curr += dir;
                }
            }
        }
        return mask;
    }

    /*
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
    */

    //L: Below is for the player to interact with the light, but it's kinda useless since we're not doing that anymore
    /*
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
    */
}
