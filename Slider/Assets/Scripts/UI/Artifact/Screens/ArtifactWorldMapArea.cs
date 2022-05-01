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
    public Image image;
    public Sprite emptySprite;
    public Sprite silhouetteSprite;
    public Sprite oneBitSprite;
    public Sprite colorSprite;

    public Material whiteSpriteMat;


    public bool SetStatus(AreaStatus status)
    {
        // don't downgrade
        if ((int)status < (int)areaStatus)
        {
            return false;
        }

        areaStatus = status;
        UpdateSprite();
        if (gameObject.activeInHierarchy)
            StartCoroutine(FlashWhite());
        return true;
    }

    public void ClearStatus()
    {
        areaStatus = AreaStatus.none;
        UpdateSprite();
    }

    public void UpdateSprite()
    {
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
    }

    private IEnumerator FlashWhite()
    {
        Debug.Log(name + "starting");
        image.material = whiteSpriteMat;

        yield return new WaitForSeconds(0.05f);
        Debug.Log(name + "ending");

        image.material = null;
    }
}
