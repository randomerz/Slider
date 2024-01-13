using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesertScrollFragTabHintPrompt : MonoBehaviour
{
    private bool promptedPreviously = false;

    [SerializeField] private PlayerActionHints playerActionHints;

    private void OnEnable()
    {
        if (!promptedPreviously)
        {
            playerActionHints.TriggerHint("scrollscrap");
            promptedPreviously = true;
        }
    }
}
