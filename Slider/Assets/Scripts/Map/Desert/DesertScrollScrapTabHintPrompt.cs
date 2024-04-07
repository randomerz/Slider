using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertScrollScrapTabHintPrompt : MonoBehaviour
{
    private bool promptedPreviously = false;

    [SerializeField] private PlayerActionHints playerActionHints;

    private void OnEnable()
    {
        if (!promptedPreviously && PlayerInventory.Contains("Scroll Scrap"))
        {
            playerActionHints.TriggerHint("scrollscrap");
            promptedPreviously = true;
        }
    }
}
