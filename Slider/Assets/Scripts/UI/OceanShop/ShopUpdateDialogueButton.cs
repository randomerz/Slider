using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopUpdateDialogueButton : MonoBehaviour
{
    public ShopDialogueManager dialogueManager;

    public void UpdateDialogueWithName()
    {
        dialogueManager.UpdateDialogue(name);
    }
}
