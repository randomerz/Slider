using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CaveLight : MonoBehaviour
{
    public bool lightOn;
    public bool lightOnStart;

    public enum LightType
    {
        UniDirectional,
        AllDirections,
    }
    public LightType type;

    public class LightEventArgs
    {
    }
    public static event System.EventHandler<LightEventArgs> lightOnEvent;
    public static event System.EventHandler<LightEventArgs> lightOffEvent;

    // Start is called before the first frame update
    void Start()
    {
        SetLightOn(lightOnStart);
    }

    void OnDisable()
    {
        SetLightOn(false);
    }

    public void SetLightOn(bool value)
    {
        lightOn = value;

        if (LightManager.instance != null)
        {
            LightManager.instance.GenerateLightMask();
            LightManager.instance.UpdateMaterials();
            if (value)
            {
                lightOnEvent?.Invoke(this, new LightEventArgs { });
            }
            else
            {
                lightOffEvent?.Invoke(this, new LightEventArgs { });
            }
        }
    }

    /* L: Gets the light mask for THIS LIGHT ONLY (see LightManager.cs for the whole world) */
    public Texture2D GetLightMask(Texture2D heightMask, int worldToMaskDX, int worldToMaskDY, int maskSizeX, int maskSizeY)
    {
        if (heightMask.width != maskSizeX && heightMask.height != maskSizeY)
        {
            Debug.LogError("heightMask did not match expected dimensions in CaveLight.cs");
        }

        Texture2D mask = new Texture2D(maskSizeX, maskSizeY);
        for (int x = 0; x < maskSizeX; x++)
        {
            for (int y = 0; y < maskSizeY; y++)
            {
                mask.SetPixel(x, y, Color.black);
            }
        }
        Vector2Int lightPos = new Vector2Int((int)transform.position.x, (int)transform.position.y);

        bool lightOnWall = heightMask.GetPixel(lightPos.x + worldToMaskDX, lightPos.y + worldToMaskDY).r > 0.5; 

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int dir in dirs)
        {
            Vector2Int curr = lightPos;
            for (int width = -8; width <= 8; width++)
            {
                //L: The current tile being observed in world coords.
                curr = lightPos + new Vector2Int(dir.y, dir.x) * width;

                //L: "Fake Raycast" from the light's position (+ width) up to 25 tiles before it hits a wall
                for (int j=0; j<=17+8; j++)
                {
                    int maskX = curr.x + worldToMaskDX;
                    int maskY = curr.y + worldToMaskDY;

                    //L: Bounds Check
                    if (maskX < 0 || maskX > maskSizeX-1 || maskY < 0 || maskY > maskSizeY-1)
                    {
                        break;
                    }
                    
                    mask.SetPixel(maskX, maskY, Color.white);

                    // L: Hit Wall Check (Note: This is after so that the start of the tile still gets lit, but nothing else.
                    if (!lightOnWall && heightMask.GetPixel(maskX, maskY).r > 0.5)
                    {
                        break;
                    }
                    curr += dir;
                }
            }
        }

        mask.Apply();
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
