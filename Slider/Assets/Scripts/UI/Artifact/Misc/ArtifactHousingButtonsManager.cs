using UnityEngine;
using UnityEngine.UI;

public class ArtifactHousingButtonsManager : MonoBehaviour 
{
    public UIArtifact uiArtifact;
    public Image background;

    public Sprite defaultBackgroundSprite;
    public Sprite housingBackgroundSprite;

    private void Start()
    {
        if (Player.GetIsInHouse())
        {
            SetSpritesToHousing(true);
        }
    }

    // This is called by the ladders in jungle D:
    public void SetSpritesToHousing(bool isInHousing)
    {
        foreach (ArtifactTileButton b in uiArtifact.buttons)
        {
            if (isInHousing)
            {
                b.GetComponent<ArtifactTBPluginHousingSprites>().UseHousingSprite();
            } 
            else
            {
                b.RestoreDefaultIslandSprite();
                b.RestoreDefaultCompletedSprite();
            }

            b.SetSpriteToIslandOrEmpty();
        }
        background.sprite = isInHousing ? housingBackgroundSprite : defaultBackgroundSprite;
    }
}