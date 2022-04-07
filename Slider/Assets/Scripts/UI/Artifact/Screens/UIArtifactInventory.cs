using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIArtifactInventory : MonoBehaviour
{
    public List<ArtifactInventoryCollectible> collectibles;

    public ArtifactInventoryCollectible anchorCollectible; // the check is player.pickedupanchor
    public ArtifactInventoryCollectible scrollCollectible; // swaps between scrap and scroll
    // boots can just be done with the other collectibles

    public TextMeshProUGUI inventoryText;

    private void OnEnable() 
    {
        UpdateIcons();
        UpdateText("Inventory");
    }

    public void UpdateIcons()
    {
        foreach (ArtifactInventoryCollectible c in collectibles)
        {
            c.SetVisible(PlayerInventory.Contains(c.collectibleName));
        }

        anchorCollectible.SetVisible(PlayerInventory.GetHasCollectedAnchor());
        
        scrollCollectible.SetVisible(PlayerInventory.Contains(scrollCollectible.collectibleName));
    }

    public void UpdateText(string text)
    {
        inventoryText.text = text;
    }
}
