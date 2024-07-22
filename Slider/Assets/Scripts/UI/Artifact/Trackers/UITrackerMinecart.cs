using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITrackerMinecart : UITracker
{
    public Minecart minecart;
    public Sprite trackerSpriteEmpty;
    public Sprite trackerSpriteLava;
    public Sprite trackerSpriteCrystal;
    public Sprite trackerSpriteEmptyWhite;
    public Sprite trackerSpriteLavaWhite;
    public Sprite trackerSpriteCrystalWhite;

    void Update()
    {
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        bool isNotOnTile = SGrid.GetSTileUnderneath(minecart.transform, minecart.currentSTile) == null;
        if (isNotOnTile)
        {
            switch (minecart.mcState)
            {
                case MinecartState.Empty:
                    image.sprite = trackerSpriteEmptyWhite;
                    break;
                case MinecartState.Lava:
                    image.sprite = trackerSpriteLavaWhite;
                    break;
                case MinecartState.Crystal:
                    image.sprite = trackerSpriteCrystalWhite;
                    break;
            }
        }
        else
        {
            switch (minecart.mcState)
            {
                case MinecartState.Empty:
                    image.sprite = trackerSpriteEmpty;
                    break;
                case MinecartState.Lava:
                    image.sprite = trackerSpriteLava;
                    break;
                case MinecartState.Crystal:
                    image.sprite = trackerSpriteCrystal;
                    break;
            }
        }
    }
}
