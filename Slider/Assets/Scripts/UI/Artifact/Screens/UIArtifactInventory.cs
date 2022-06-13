using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIArtifactInventory : MonoBehaviour
{
    public List<ArtifactInventoryCollectible> collectibles;

    public ArtifactInventoryCollectible anchorCollectible; // the check is player.pickedupanchor
    public ArtifactInventoryCollectible scrollCollectible; 
    public ArtifactInventoryCollectible scrollFragCollectible;

    public TextMeshProUGUI inventoryText;


    [Header("Special Collectible Counters")] // could be refactored
    public TextMeshProUGUI flashlightCount;

    private void OnEnable() 
    {
        UpdateIcons();
        UpdateText("Inventory");

        UpdateCollectibleCounters(this, null);
        PlayerInventory.OnPlayerGetCollectible += UpdateCollectibleCounters;
    }

    private void OnDisable() 
    {
        PlayerInventory.OnPlayerGetCollectible -= UpdateCollectibleCounters;
    }

    public void UpdateIcons()
    {
        foreach (ArtifactInventoryCollectible c in collectibles)
        {
            c.SetVisible(PlayerInventory.Contains(c.collectibleName));
        }

        anchorCollectible.SetVisible(PlayerInventory.Instance.GetHasCollectedAnchor());

        scrollCollectible.SetVisible(PlayerInventory.Contains("Scroll of Realigning"));
        scrollFragCollectible.SetVisible(!PlayerInventory.Contains("Scroll of Realigning") && PlayerInventory.Contains("Scroll Frag"));
    }

    public void UpdateText(string text)
    {
        inventoryText.text = text;
    }


    private void UpdateCollectibleCounters(object sender, PlayerInventory.InventoryEvent e)
    {
        int numFlashlight = 0;
        if (PlayerInventory.Contains("Flashlight", Area.Village)) numFlashlight += 1;
        if (PlayerInventory.Contains("Flashlight", Area.Caves))   numFlashlight += 1;
        
        flashlightCount.text = numFlashlight.ToString();
        flashlightCount.gameObject.SetActive(numFlashlight > 1);
    }
}
