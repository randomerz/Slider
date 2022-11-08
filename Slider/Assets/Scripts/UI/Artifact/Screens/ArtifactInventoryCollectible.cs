using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArtifactInventoryCollectible : MonoBehaviour
{
    [Tooltip("Name that shows up in the inventory screen")]
    public string displayName;
    [Tooltip("ID of the collectible to be saved")]
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
