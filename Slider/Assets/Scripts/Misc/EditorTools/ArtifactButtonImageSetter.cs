using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactButtonImageSetter : MonoBehaviour
{
    public ArtifactTileButton[] buttons;
    public Image[] images;
    public Sprite[] islandSprites;
    public Sprite[] completedSprites;

    
    public void FindImagesList()
    {
        images = new Image[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            Transform imageTransform = buttons[i].transform.GetChild(0).GetChild(0);
            images[i] = imageTransform.GetComponent<Image>();
        }
    }

    public void UpdateImages()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].islandSpriteDefault = islandSprites[i];
            buttons[i].completedSprite = completedSprites[i];
            images[i].sprite = islandSprites[i];
        }
    }
}
