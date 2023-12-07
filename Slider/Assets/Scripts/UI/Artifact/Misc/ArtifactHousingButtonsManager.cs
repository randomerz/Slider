using UnityEngine;
using UnityEngine.UI;

public class ArtifactHousingButtonsManager : MonoBehaviour 
{
    public UIArtifact uiArtifact;
    public Image background;

    public Sprite defaultBackgroundSprite;
    public Sprite housingBackgroundSprite;

    [SerializeField] private JungleButtonMiddleAnimator connectorAnimator;

    public Sprite defaultConnectorSprite;
    public Sprite housingConnectorSprite;

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
        if (connectorAnimator != null)
        {
            connectorAnimator.ConnectorSprite = isInHousing ? housingConnectorSprite : defaultConnectorSprite;
        }
    }
}