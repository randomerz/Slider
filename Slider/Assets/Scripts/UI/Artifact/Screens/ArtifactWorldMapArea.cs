using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactWorldMapArea : MonoBehaviour
{
    public enum AreaStatus
    {
        none,
        silhouette,
        oneBit,
        color,
    }

    private AreaStatus areaStatus;

    // references
    public Area myArea;
    public Image image;
    public Image playerPin;
    public UIArtifactWorldMap worldmap;

    public Sprite emptySprite;
    public Sprite silhouetteSprite;
    public Sprite oneBitSprite;
    public Sprite colorSprite;
    public Sprite controllerSelectSprite;

    public Material whiteSpriteMat;

    public Selectable controllerSelectible;

    private bool needsToUpdateSprite = false;

    private void Start() 
    {
        playerPin.gameObject.SetActive(SGrid.Current.GetArea() == myArea);
    }

    public void SelectCurrentArea(string controls)
    {
        if(controls != Controls.CONTROL_SCHEME_CONTROLLER) return;
        if(SGrid.Current.GetArea() == myArea)
        {
            controllerSelectible.Select();
            OnSelect();
        }

    }

    private void OnEnable()
    {
        SelectCurrentArea(Controls.CurrentControlScheme);
    }

    private void OnDisable() 
    {
        StopAllCoroutines();
        image.material = null;
        playerPin.material = null;
    }

    public bool SetStatus(AreaStatus status)
    {
        // don't downgrade
        if ((int)status < (int)areaStatus)
        {
            return false;
        }

        areaStatus = status;
        needsToUpdateSprite = true;
        if(Controls.CurrentControlScheme == Controls.CONTROL_SCHEME_CONTROLLER)
            ToggleControllerSelect(true);
        return true;
    }

    public void ClearStatus()
    {
        areaStatus = AreaStatus.none;
        needsToUpdateSprite = true;
    }

    public void UpdateSprite(bool force=false)
    {
        if (!needsToUpdateSprite && !force)
            return;
        needsToUpdateSprite = false;

        switch (areaStatus)
        {
            case AreaStatus.none:
                image.sprite = emptySprite;
                break;
            case AreaStatus.silhouette:
                image.sprite = silhouetteSprite;
                break;
            case AreaStatus.oneBit:
                image.sprite = oneBitSprite;
                break;
            case AreaStatus.color:
                image.sprite = colorSprite;
                break;
        }
        
        if (gameObject.activeInHierarchy)
            StartCoroutine(FlashWhite(3));
    }

    private IEnumerator FlashWhite(int n)
    {
        for (int i = 0; i < n; i++)
        {
            image.material = whiteSpriteMat;
            playerPin.material = whiteSpriteMat;

            yield return new WaitForSeconds(0.25f);

            image.material = null;
            playerPin.material = null;

            yield return new WaitForSeconds(0.25f);
        }
    }

    public void UpdateAreaName()
    {
        if (areaStatus >= AreaStatus.silhouette)
        {
            worldmap.UpdateText(myArea.GetDisplayName());
        }
    }

    public void ToggleControllerSelect(bool val)
    {
        if(controllerSelectible == null) return;
        var nav = controllerSelectible.navigation;
        nav.mode = val && areaStatus >= AreaStatus.silhouette ? Navigation.Mode.Automatic : Navigation.Mode.None;
        controllerSelectible.navigation = nav;
    }

    public void OnSelect()
    {
        if(Controls.CurrentControlScheme != Controls.CONTROL_SCHEME_CONTROLLER) return;
        worldmap.UpdateControllerSelectionSprite(controllerSelectSprite);
        UpdateAreaName();
    }

    public void OnDeselect()
    {
        worldmap.ClearControllerSelectionSprite();
    }

}
