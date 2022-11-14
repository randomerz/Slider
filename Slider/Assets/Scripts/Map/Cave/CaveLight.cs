using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CaveLight : MonoBehaviour
{

    public bool LightOn { get; private set; }

    [SerializeField]
    internal bool lightOnStart;

    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;

    [SerializeField] private Vector2Int borderPos;

    public class OnLightSwitchedArgs
    {
        public bool lightOn;
    }
    public static event System.EventHandler<OnLightSwitchedArgs> OnLightSwitched;

    void OnEnable()
    {
        SetLightOn(lightOnStart);
    }

    void OnDisable()
    {
        SetLightOn(false);
    }

    public bool LightActiveInScene()
    {
        STile lOnTile = GetComponentInParent<STile>();
        return lOnTile == null || lOnTile.isTileActive;
    }

    public void SetLightOn(bool value, bool playSound=false)
    {
        spriteRenderer.sprite = value ? onSprite : offSprite;
        if (LightOn != value)
        {
            LightOn = value;

            if (playSound)
            {
                if (value)
                    AudioManager.Play("Power On"); 
                else
                    AudioManager.Play("Power Off"); 
            }

            OnLightSwitched?.Invoke(this, new OnLightSwitchedArgs { lightOn = value });
        }
    }

    // DC: for some reason SetLightOn can't be exposed with default values to unityevents
    public void SetLightWithSound(bool value)
    {
        SetLightOn(value, true);
    }

    public void SetLightWithoutSound(bool value)
    {
        SetLightOn(value, false);
    }

    public void SetLightOnPowered(ElectricalNode.OnPoweredArgs e)
    {
        SetLightWithSound(e.powered);
    }

    //Idk if this needs to be pass by reference, but we def. don't want to copy the whole array.
    public void UpdateLightMask(ref Color[] lightMask, Texture2D heightMask, int worldToMaskDX, int worldToMaskDY, int maskSizeX, int maskSizeY)
    {
        Vector2Int lightPos = new Vector2Int((int)transform.position.x, (int)transform.position.y);

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int dir in dirs)
        {
            for (int width = -8; width <= 8; width++)
            {
                //L: The current tile being observed in world coords.
                Vector2Int curr = lightPos + new Vector2Int(dir.y, dir.x) * width;

                //L: "Fake Raycast" from the light's position (+ width) up to 25 tiles before it hits a wall
                for (int j=0; j<=17+8; j++)
                {
                    int maskX = curr.x + worldToMaskDX;
                    int maskY = curr.y + worldToMaskDY;

                    //L: Bounds Check
                    if (maskX < 0 || maskX >= maskSizeX || maskY < 0 || maskY >= maskSizeY)
                    {
                        break;
                    }
                    
                    lightMask[maskY * maskSizeX + maskX] = Color.white;

                    // L: Hit Wall Check (Note: This is after so that the start of the tile still gets lit, but nothing else.
                    if (heightMask.GetPixel(maskX, maskY).r > 0.5)
                    {
                        break;
                    }
                    curr += dir;
                }
            }
        }
    }

    public Vector2Int GetPos()
    {
        STile stile = GetComponentInParent<STile>();

        return stile == null ? borderPos : new Vector2Int(stile.x, stile.y);
    }
}
