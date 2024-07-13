using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopUpdateDialogueButton : MonoBehaviour
{
    public ShopDialogueManager dialogueManager;

    public void UpdateDialogueWithName()
    {
        if (ShopManager.Instance.IsSwitchPanelBufferOn())
        {
            Debug.Log($"[Ocean Shop] Skipped input because buffer was on.");
            return;
        }
        
        dialogueManager.UpdateDialogue(name);
    }
}
