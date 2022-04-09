using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactInventoryCollectible : MonoBehaviour
{
    public string displayName;
    public string collectibleName;

    public UIArtifactInventory inventory;

    [HideInInspector] public bool isVisible;

    public void SetVisible(bool value)
    {
        isVisible = value;
        gameObject.SetActive(value);
    }

    public void UpdateInventoryName()
    {
        inventory.UpdateText(displayName);
    }
}
