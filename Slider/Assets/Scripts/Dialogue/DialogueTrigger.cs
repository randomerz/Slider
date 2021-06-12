using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;

    //Call to dialogueManager function
    public void TriggerDialogue()
    {
        FindObjectOfType<DialogueManager>().DisplayDialogues(dialogue);
    }
}